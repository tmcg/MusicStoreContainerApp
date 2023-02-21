
# Deploying Music Store on Azure Containers Apps

## Pre-requisites

- Azure CLI v2.45+
- Docker Desktop v4.16+
- A private Azure Container Registry

### Install Azure CLI Extensions

```powershell
az extension add --name containerapp --upgrade
az extension add --name log-analytics --upgrade
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
```

### Build the Container Application

```powershell
cd src
docker build -t musicstore -f ./MusicStore/Dockerfile .
```

### Push Container to Azure Container Registry

First, retrieve an ACR access token and use this to login via Docker Desktop

```powershell
$RegistryServer = 'tmcgregistry.azurecr.io'
$RegistryName = ($RegistryServer -split '\.')[0]

# Use a generated Registry Access Token, and Login via Docker
$RegistryUserName = '00000000-0000-0000-0000-000000000000'
$RegistryPassword = (az acr login -n $RegistryName --expose-token --output tsv --query accessToken)
$RegistryPassword | docker login $RegistryServer --username $RegistryUserName --password-stdin

# Tag and push the Image to ACR, then delete the local image
$AppImageName = "$RegistryServer/musicstore:v1"
docker tag musicstore:latest $AppImageName
docker push $AppImageName
docker rmi $AppImageName
```

### Deploying Container App Manually via CLI

```powershell
# Requires Admin Enabled on Azure Container Registry
# az acr update -n $RegistryName --admin-enabled true
# az acr credential show -n $RegistryName

$RegistryServer = 'tmcgregistry.azurecr.io'
$RegistryName = ($RegistryServer -split '\.')[0]
$RegistryUserName = (az acr credential show -n $RegistryName --output tsv --query username)
$RegistryPassword = (az acr credential show -n $RegistryName --output tsv --query 'passwords[0].value')
$Location = 'australiaeast'
$ResourceGroup = 'MusicStore-RG'
$AppImageName = "$RegistryServer/musicstore:v1"
$AppEnvironment = 'MusicStoreEnv'
$AppName = 'musicstore'

# Create the Resource Group
az group create -n $ResourceGroup -l $Location

# Create the Container App Environment
az containerapp env create -n $AppEnvironment -g $ResourceGroup -l $Location

# Create the Container App
az containerapp create -n $AppName -g $ResourceGroup --image $AppImageName --environment $AppEnvironment --registry-server $RegistryServer --registry-username $RegistryUserName --registry-password $RegistryPassword

# Enable HTTPS Ingress 
az containerapp ingress enable -n $AppName -g $ResourceGroup --type external --target-port 80

# Launch the Container App in a Browser
$AppUrl = "https://musicstore--j4fn92y.lemondesert-d4079eb1.australiaeast.azurecontainerapps.io/"
Invoke-Item $AppUrl

# Verify the Deployment
$CustomerId = (az containerapp env show -n $AppEnvironment -g $ResourceGroup --query properties.appLogsConfiguration.logAnalyticsConfiguration.customerId --out tsv)

$LogFilter = "where ContainerAppName_s == '$AppName'"
$LogProject = "project ContainerAppName_s, Log_s, TimeGenerated"
$LogQuery = "ContainerAppConsoleLogs_CL | $LogFilter | $LogProject"
az monitor log-analytics query --workspace $CustomerId --analytics-query $LogQuery --out table
```


