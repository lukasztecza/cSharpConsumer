#!/usr/bin/env bash

# Set versions
MYSQL_ROOT_PASSWORD=pass
MYSQL_USER=user
MYSQL_USER_PASSWORD=pass
MYSQL_HOST=localhost
MYSQL_DATABASE=some_db

apt-get update

# Set mysql answers and install mysql-server and mysql-client
debconf-set-selections <<< "mysql-server mysql-server/root_password password $MYSQL_ROOT_PASSWORD"
debconf-set-selections <<< "mysql-server mysql-server/root_password_again password $MYSQL_ROOT_PASSWORD"
apt-get install -y mysql-server-5.7 mysql-client-5.7

# Set up database (note no space after -p)
mysql -u root -p"$MYSQL_ROOT_PASSWORD" <<EOL
CREATE DATABASE IF NOT EXISTS $MYSQL_DATABASE CHARACTER SET utf8 COLLATE utf8_general_ci;
GRANT ALL PRIVILEGES ON $MYSQL_DATABASE.* TO $MYSQL_USER@$MYSQL_HOST IDENTIFIED BY '$MYSQL_USER_PASSWORD';
FLUSH PRIVILEGES;
EOL

if [ $? != "0" ]; then
    echo "[Error]: Database creation failed. Aborting."
    exit 1
fi

# Install rabbitmq and allow web interface
sudo apt-get install -y erlang-nox
sudo apt-get install -y rabbitmq-server
sudo rabbitmq-plugins enable rabbitmq_management
sudo rabbitmqctl add_user test test
sudo rabbitmqctl set_user_tags test administrator
sudo rabbitmqctl set_permissions -p / test ".*" ".*" ".*"

# Install dotnet
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-2.1

cd /vagrant

# Create sample table in db
mysql -u "$MYSQL_USER" -p"$MYSQL_USER_PASSWORD" -h $MYSQL_HOST $MYSQL_DATABASE < db.sql

sudo reboot

echo "Use vagrant halt to stop machine vagrant up to start machine vagrant ssh to enter machine"
