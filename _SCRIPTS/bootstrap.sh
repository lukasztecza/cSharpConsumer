#!/usr/bin/env bash

# Add microsoft key
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -

# Install mssql-server and mssql client
if ! systemctl status mssql-server status | grep 'Active: active (running)'; then
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/16.04/mssql-server-2017.list)"
sudo apt-get update
sudo apt-get install -y mssql-server
ACCEPT_EULA=Y MSSQL_PID=Developer MSSQL_LCID=1033 MSSQL_SA_PASSWORD="MSSQLadmin!" sudo -E /opt/mssql/bin/mssql-conf setup
sudo systemctl restart mssql-server.service

sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/16.04/prod.list)"
sudo apt-get update
ACCEPT_EULA=Y DEBIAN_FRONTEND=noninteractive sudo -E apt-get install -y mssql-tools unixodbc-dev
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc
fi

# Install rabbitmq and allow web interface
if ! sudo rabbitmqctl status | grep 'rabbit@ubuntu-xenial'; then
sudo apt-get install -y erlang-nox
sudo apt-get install -y rabbitmq-server
sudo rabbitmq-plugins enable rabbitmq_management
sudo rabbitmqctl add_user test test
sudo rabbitmqctl set_user_tags test administrator
sudo rabbitmqctl set_permissions -p / test ".*" ".*" ".*"
sudo rabbitmqctl add_vhost /analytics
sudo rabbitmqctl set_permissions -p /analytics test ".*" ".*" ".*"
fi

# Install dotnet
if ! dotnet --info status | grep '.NET Core SDK'; then
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-2.1
fi

# Create database
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'MSSQLadmin!' < /vagrant/_SCRIPTS/db_schema.sql

echo "Use vagrant halt to stop machine vagrant up to start machine vagrant ssh to enter machine"
echo "You may want to exectue sample sql file db.sql - in order to do it ssh to the machine and run"
echo "/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'MSSQLadmin!' < db.sql"
echo "Open rabbit user interface in your browser at localhost:15672 - user is test and password is test"
