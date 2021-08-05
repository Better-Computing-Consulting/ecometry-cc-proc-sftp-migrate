# ecometry-cc-proc-sftp-migrate
Program to replace ftp with sftp on credit card authorization steps for the Red Prairie Ecometry to migrate to a new credit card vendor.

The program sets its variables from command line arguments.  Depending on the arguments the program will run on one of three modes, submit, retrieve or settle credit card charges.

There is one batch file, with passes different arguments to the program, for each mode, and with are schedule for automatic execution.

The program sends email notifications with the results of each run.
