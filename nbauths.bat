    @ECHO OFF
    CLS
    SET PUB=E:\ECOMETRY\ECOMVER\BATCHFILES\
    SET SPOOLOUT=E:\ECOMETRY\ECOMVER\SPOOLOUT\
    SET TEMP=E:\ECOMETRY\ECOMVER\temp\
    SET REMOTEDIR=/MSOD-To-FDC
    SET sftpUID=ssss
    SET sftpPW=sssssss
    SET sftpHost=3.3.3.3
    SET LOCALFILE=%PUB%nbauthot
    
REM * The nbauths.txt and .err files are used if the nbauths file isn't found:
      if exist %TEMP%nbauths.txt DEL %TEMP%nbauths.txt
      if exist %TEMP%nbauths.err DEL %TEMP%nbauths.err

REM * Ensure That The nbauths file exists in PUB:
      DIR %LOCALFILE% >%TEMP%nbauths.txt
      IF NOT %ERRORLEVEL% == 0 GOTO :NONBAUTHS

REM * nbauthsftp.log is used to display the ftp session to Nabanco:
      IF EXIST %SPOOLOUT%nbauthsftp.log DEL %SPOOLOUT%nbauthsftp.log 

:FTPFile
      ECHO Starting SFTP... Please Wait... >%TEMP%nbauthsftp.log
      ppsftp %PUB% %TEMP% %REMOTEDIR% %sftpUID% %sftpPW% %sftpHost% %LOCALFILE% >>%TEMP%nbauthsftp.log
      echo %ERRORLEVEL% >>%TEMP%nbauthsftp.log
      IF NOT %ERRORLEVEL% == 0 GOTO :FTPFAILED
      IF EXIST %TEMP%ftp.txt DEL %TEMP%ftp.txt
      ECHO SUCCESS >%TEMP%nbauths.chk
      GOTO :EOF
:NONBAUTHS
      DIR %LOCALFILE% >%TEMP%nbauths.chk
      ECHO %LOCALFILE% Not Found. >>%TEMP%nbauths.chk
      ppsftp AUTH_NBAUTHS_FILE_NOT_FOUND
      GOTO :EOF
:FTPFAILED
      ECHO FTP Failed >%TEMP%nbauths.chk
      TYPE %TEMP%nbauthsftp.log >>%TEMP%nbauths.chk
      ppsftp AUTH_PUT_SFTP_FAILED
      GOTO :EOF

