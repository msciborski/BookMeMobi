pip install --user awscli
export PATH=$PATH:$HOME/.local/bin
aws configure set region eu-central-1
$(aws ecr get-login --no-include-email --region eu-central-1)
docker build -t bookmemobi2 .
docker ps
docker tag bookmemobi2:latest 601510060817.dkr.ecr.eu-central-1.amazonaws.com/bookmemobi2:latest
docker push 601510060817.dkr.ecr.eu-central-1.amazonaws.com/bookmemobi2:latest
aws ecs stop-task --cluster bookmemobi-cluster --task $(aws ecs list-tasks --cluster bookmemobi-cluster --output text --query taskArns[0])
aws ecs run-task --cluster bookmemobi-cluster --task-definition bookmemobi2-task-definition:4