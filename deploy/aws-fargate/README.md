
# Deploying Music Store on AWS EC2 Fargate

## Pre-requisites

- AWS CLI v2.45+
- Docker Desktop v4.16+
- A private Elastic Container Registry repository

### Build the Container Application

```powershell
cd src
docker build -t musicstore -f ./MusicStore/Dockerfile .
```

### Create Elastic Container Registry repository

```powershell
$Region = 'ap-southeast-2'
$RepositoryName = 'musicstore'

aws ecr create-repository --repository-name $RepositoryName --region $Region
```

### Push Container to Elastic Container Registry repository

```powershell
$AccountId = '012345678901'
$Region = 'ap-southeast-2'
$RegistryServer = "${AccountId}.dkr.ecr.${Region}.amazonaws.com"
$RepositoryName = 'musicstore'
$AppImageName = "$RegistryServer/$RepositoryName"

# Get the repository login password, and Login via Docker
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $RegistryServer

# Tag and push the Image to ECR, then delete the local tag
docker tag musicstore:latest $AppImageName
docker push $AppImageName
docker rmi $AppImageName
```

### Deploying to AWS Fargate Manually via CLI

```powershell
$AccountId = '012345678901'
$Region = 'ap-southeast-2'
$RegistryServer = "${AccountId}.dkr.ecr.${Region}.amazonaws.com"
$RepositoryName = 'musicstore'
$AppImageName = "$RegistryServer/$RepositoryName"

# Create the ECS Tasks Role, if it doesn't exist
aws iam create-role --role-name ecsTaskExecutionRole --assume-role-policy-document file://policy-assume-role.json
# This role must also have ECR and CloudWatch permissions
aws iam put-role-policy --role-name ecsTaskExecutionRole --policy-name ecsTaskExecutionPolicy --policy-document file://policy-task-role.json

# Create the CloudWatch logs group
# Alternatively, specify awslogs-create-group="true" under log configuration options in the task definition
$LogGroupName = '/ecs/sample-musicstore'
aws logs create-log-group --log-group-name $LogGroupName --region $Region

# Create the Elastic Container Service cluster
$EcsClusterName = 'musicstore-cluster'
aws ecs create-cluster --cluster-name $EcsClusterName --region $Region

# Register the ECS Task Definition
cd deploy/aws-fargate
aws ecs register-task-definition --cli-input-json file://task-definition.json

# List the ECS Task Definitions and describe available subnets
aws ecs list-task-definitions
aws ec2 describe-subnets --query 'Subnets[].[VpcId,Tags[?Key==`Name`]|[0].Value,SubnetId,CidrBlock]' --output text | sort
aws ec2 describe-security-groups --query 'SecurityGroups[].[VpcId,Tags[?Key==`Name`]|[0].Value,GroupId]' --output text | sort
# ... select subnet(s) and security group(s) for the ECS service ...
# ... must have route to public internet for ECR and inbound port 80 ...

# Create the ECS Service
$EcsClusterName = 'musicstore-cluster'
$ServiceName = 'musicstore-svc'
$TaskDefinitionName = 'sample-musicstore:2'
$NetworkConfig = 'awsvpcConfiguration={subnets=[subnet-0131de856918ebeaa,subnet-05504b926daf9352e],securityGroups=[sg-0da9e930320a7599c,sg-098a8525e2795076c],assignPublicIp=ENABLED}'
aws ecs create-service --cluster $EcsClusterName --service-name $ServiceName --task-definition $TaskDefinitionName --desired-count 1 --launch-type FARGATE --network-configuration $NetworkConfig

# Optionally, specify a load balancer that ECS will register with when tasks run.
# Container can then be placed into a private subnet, and assignPublicIp can be disabled.
# Container name and port are from task-definition.json
$TargetGroupArn = "arn:aws:elasticloadbalancing:${Region}:${AccountId}:targetgroup/MusicStoreTG/1457b5e06300bbae"
$LoadBalancers = "targetGroupArn=$TargetGroupArn,containerName=musicstore-app,containerPort=80"
aws ecs update-service --cluster $EcsClusterName --service $ServiceName --load-balancers $LoadBalancers

# Optionally, increase the desired count. Containers will distribute among subnets.
aws ecs update-service --cluster $EcsClusterName --service $ServiceName --desired-count 4

# Launch the Container App in a browser
# -- via public IP of ECS container
Invoke-Item "http://3.26.175.209/"
# -- via DNS name of associated ALB (optionally terminate TLS on this endpoint)
Invoke-Item "http://musicstorealb-1550323131.ap-southeast-2.elb.amazonaws.com/" 

# Verify the Deployment
aws ecs describe-services --cluster $EcsClusterName --services $ServiceName
aws logs tail $LogGroupName --follow
```
