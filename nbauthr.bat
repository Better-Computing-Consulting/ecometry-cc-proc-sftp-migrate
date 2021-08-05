    @ECHO OFF
    CLS
    SET PUB=E:\ECOMETRY\ECOMVER\BATCHFILES\
    SET SPOOLOUT=E:\ECOMETRY\ECOMVER\SPOOLOUT\
    SET TEMP=E:\ECOMETRY\ECOMVER\TEMP\
    SET sftpFILE=AUDXMPPB
    SET sftpUID=ssss
    SET sftpPW=ssssss
    SET sftpHost=3.3.3.3
    SET LOCALFILE=%PUB%nbauthin

REM * nbauthr.chk is used by the calling stream to determine SUCCESS or FAILURE:
      IF EXIST %TEMP%nbauthr.chk DEL %TEMP%nbauthr.chk
REM * Ensure That The nbauthin file doesn't exist in PUB:
      IF EXIST %LOCALFILE% DEL %LOCALFILE%
      IF EXIST %LOCALFILE% GOTO :NODELETE
REM * nbauthrftp.log is used to display the ftp session to Nabanco:
      IF EXIST %SPOOLOUT%nbauthrftp.log DEL %SPOOLOUT%nbauthrftp.log 

:FTPFile
      ECHO Starting FTP... Please Wait... >%TEMP%nbauthrftp.log
      ppsftp %PUB% %TEMP% %sftpFILE% %sftpUID% %sftpPW% %sftpHost% %LOCALFILE% >>%TEMP%nbauthrftp.log
      IF NOT %ERRORLEVEL% == 0 GOTO :FTPFAILED
      IF NOT EXIST %LOCALFILE% GOTO :NOFILERECEIVED
      DIR %LOCALFILE% >>%TEMP%ftp.txt
      ECHO SUCCESS >%TEMP%nbauthr.chk
      GOTO :EOF
:NODELETE
      ECHO %LOCALFILE% Cannot Be Deleted!. >>%TEMP%nbauthr.chk
		ppsftp GET_AUTH_PRE_DELETE_FAILED
      GOTO :EOF
:FTPFAILED
      ECHO FTP Failed >%TEMP%nbauthr.chk
      TYPE %TEMP%nbauthrftp.log >>%TEMP%nbauthr.chk
		ppsftp GET_SFTP_FAILED
      GOTO :EOF
:NOFILERECEIVED
      ECHO FTP Failed - NO File Retrieved. >%TEMP%nbauthr.chk
      TYPE %TEMP%nbauthrftp.log >>%TEMP%nbauthr.chk
      DEL %TEMP%nbauthrftp.log
		ppsftp GET_NBAUTHIN_FAILED
      GOTO :EOF
