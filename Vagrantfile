# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant::DEFAULT_SERVER_URL.replace('https://vagrantcloud.com')
Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/xenial64"
  config.vm.box_version = "20181004.0.0"
  config.vm.network "forwarded_port", guest: 15672, host: 15672
  config.vm.synced_folder ".", "/vagrant", :owner=> 'www-data', :group=>'vagrant', :mount_options => ["dmode=775","fmode=664"]
  config.vm.provider :virtualbox do |vb|
    vb.customize ["modifyvm", :id, "--memory", "2304"]
  end
  config.vm.provision :shell, path: "./_SCRIPTS/bootstrap.sh"
end
