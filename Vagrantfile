# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/xenial64"
  config.vm.network "forwarded_port", guest: 15672, host: 15672
  config.vm.synced_folder ".", "/vagrant", :owner=> 'www-data', :group=>'vagrant', :mount_options => ["dmode=775","fmode=664"]
  config.vm.provider :virtualbox do |vb|
    vb.customize ["modifyvm", :id, "--memory", "1024"]
  end
  config.vm.provision :shell, path: "./bootstrap.sh"
end
