# PostgreSQL with PostGIS in AKS

## Prerequisites

Helm installed and Bitnami repo added (one-time setup):

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

You also need a PersistentVolume on a worker node before deploying. See the [PersistentVolume](#persistentvolume) section below.

---

## Important: Bitnami chart + PostGIS incompatibility

The Bitnami PostgreSQL Helm chart is **not compatible** with the `postgis/postgis` image. The chart applies a security context that makes `/var/run/postgresql` read-only, which prevents PostgreSQL from starting. This affects any non-Bitnami image.

**Do not use the Bitnami chart with PostGIS.** Use the plain StatefulSet approach below instead.

---

## Deploy PostgreSQL with PostGIS (StatefulSet)

Save the following to a file (e.g. `postgres-statefulset.yaml`) and apply it. Replace the values in angle brackets.

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: <release-name>
  namespace: <namespace>
spec:
  serviceName: <release-name>
  replicas: 1
  selector:
    matchLabels:
      app: <release-name>
  template:
    metadata:
      labels:
        app: <release-name>
    spec:
      containers:
      - name: postgres
        image: postgis/postgis:16-3.4
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_DB
          value: <database-name>
        - name: POSTGRES_USER
          value: postgres
        - name: POSTGRES_PASSWORD
          value: <password>
        - name: PGDATA
          value: /var/lib/postgresql/data/pgdata
        volumeMounts:
        - name: data
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: <release-name>-pvc
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: <release-name>-pvc
  namespace: <namespace>
spec:
  accessModes:
    - ReadWriteOnce
  storageClassName: local-hostpath
  resources:
    requests:
      storage: 5Gi
---
apiVersion: v1
kind: Service
metadata:
  name: <release-name>-postgresql
  namespace: <namespace>
spec:
  selector:
    app: <release-name>
  ports:
  - port: 5432
    targetPort: 5432
```

```bash
kubectl apply -f postgres-statefulset.yaml
```

### Key values to change

| Value | Description |
|---|---|
| `<release-name>` | Name used for the StatefulSet, Service, and PVC (e.g. `reststop-postgres`) |
| `<namespace>` | Kubernetes namespace (must already exist) |
| `<password>` | Password for the `postgres` user — save this in Vault immediately |
| `<database-name>` | Name of the default database to create (e.g. `reststop`) |

> The Service name `<release-name>-postgresql` determines the in-cluster DNS name. Keep the `-postgresql` suffix so the connection string pattern stays consistent.

---

## PersistentVolume

Your cluster uses `local-hostpath` storage which requires manually creating a PV on a specific worker node before deploying.

**Step 1** — create the directory on the worker node:

```bash
ssh <user>@<worker-node> "sudo mkdir -p /mnt/<app>-data && sudo chmod 777 /mnt/<app>-data"
```

**Step 2** — create a `pv.yaml` file on the control node and apply it:

```yaml
apiVersion: v1
kind: PersistentVolume
metadata:
  name: <release-name>-pv
spec:
  capacity:
    storage: 5Gi
  accessModes:
    - ReadWriteOnce
  persistentVolumeReclaimPolicy: Retain
  storageClassName: local-hostpath
  hostPath:
    path: /mnt/<app>-data
    type: DirectoryOrCreate
  nodeAffinity:
    required:
      nodeSelectorTerms:
      - matchExpressions:
        - key: kubernetes.io/hostname
          operator: In
          values:
          - <worker-node-name>
```

```bash
kubectl apply -f pv.yaml
```

If the PV ends up in `Released` state after a reinstall, reset it:

```bash
kubectl patch pv <release-name>-pv -p '{"spec":{"claimRef":null}}'
```

---

## Connection string

The Service name determines the internal DNS name:

```
<release-name>-postgresql.<namespace>.svc.cluster.local
```

Full connection string for your app / Vault secret:

```
Host=<release-name>-postgresql.<namespace>.svc.cluster.local;Port=5432;Database=<database-name>;Username=postgres;Password=<password>
```

**Example** (reststop in test namespace):

```
Host=reststop-postgres-postgresql.test.svc.cluster.local;Port=5432;Database=reststop;Username=postgres;Password=reststop
```

---

## Verify the pod is running

```bash
kubectl get pods -n <namespace>
```

The pod will be named `<release-name>-0`. Wait for `STATUS` to show `Running`.

If it stays `Pending`, check why:

```bash
kubectl describe pod <release-name>-0 -n <namespace>
```

Look at the `Events` section at the bottom — usually a storage/PVC binding issue.

---

## Connect to the database manually

From inside the cluster (spins up a temporary client pod):

```bash
kubectl run pg-client --rm --tty -i --restart='Never' \
  --namespace <namespace> \
  --image postgis/postgis:16-3.4 \
  --env="PGPASSWORD=<password>" \
  --command -- psql --host <release-name>-postgresql -U postgres -d <database-name> -p 5432
```

From your local machine (port-forward):

```bash
kubectl port-forward --namespace <namespace> svc/<release-name>-postgresql 5432:5432
# then in another terminal:
PGPASSWORD="<password>" psql --host 127.0.0.1 -U postgres -d <database-name> -p 5432
```

---

## Uninstall

```bash
kubectl delete -f postgres-statefulset.yaml
kubectl delete pv <release-name>-pv
```

To also wipe the data directory on the worker node:

```bash
ssh <user>@<worker-node> "sudo rm -rf /mnt/<app>-data"
```
