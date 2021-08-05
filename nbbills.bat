    @ECHO OFF
    CLS
    SET PUB=E:\ECOMETRY\ECOMVER\BATCHFILES\
    SET BURN=E:\ECOMETRY\ECOMVER\BURNBAG\
    SET SPOOLOUT=E:\ECOMETRY\ECOMVER\SPOOLOUT\
    SET TEMP=E:\ECOMETRY\ECOMVER\temp\
    SET REMOTEDIR=/MSOD-To-FDC
    SET sftpUID=MSOD-000851
    SET sftpPW=0221cat
    SET sftpHost=76.89.129.185
    SET LOCALFILE=%PUB%NBDEPOS2

REM * nbbills.chk is used by the calling stream to determine SUCCESS or FAILURE:
      IF EXIST %TEMP%nbbills.chk DEL %TEMP%nbbills.chk
REM * The nbbills.txt and .err files are used if the nbbills file isn't found:
      if exist %TEMP%nbbills.txt DEL %TEMP%nbbills.txt
      if exist %TEMP%nbbills.err DEL %TEMP%nbbills.err
REM The "nbbills.tmp" will be used to determine if the file got sent correctly:
      if exist %TEMP%nbbills.tmp DEL %TEMP%nbbills.tmp
REM The "nbbills.diff" file contains the result of a "diff" command between the file
REM Sent, and the file Received (Check To Ensure File Transferred Successfully):
      if exist %TEMP%nbbills.diff DEL %TEMP%nbbills.diff
REM * Ensure That The nbdepos2 file exists in PUB:
      echo DIR %LOCALFILE% >%TEMP%nbbills.txt
      DIR %LOCALFILE% >>%TEMP%nbbills.txt
      IF NOT %ERRORLEVEL% == 0 GOTO :NOnbbills

REM * nbbillsftp.log is used to display the ftp session to Nabanco:
      IF EXIST %TEMP%nbbillsftp.log DEL %TEMP%nbbillsftp.log 

:FTPFile
      ECHO Starting FTP... Please Wait... >%TEMP%nbbillsftp.log
		ppsftp %PUB% %TEMP% %REMOTEDIR% %sftpUID% %sftpPW% %sftpHost% %LOCALFILE% >>%TEMP%nbbillsftp.log
      IF NOT %ERRORLEVEL% == 0 GOTO :FTPFAILED
      IF EXIST %LOCALFILE% move %LOCALFILE% %BURN%NBDEPOS2 >%TEMP%nbbills.log
      echo moved the %LOCALFILE% file to %BURN%NBDEPOS2 >>%TEMP%nbbills.log
      ECHO SUCCESS >%TEMP%nbbills.chk
      GOTO :EOF
:NOnbbills
      ECHO %LOCALFILE% Not Found. >%TEMP%nbbills.chk
      type %TEMP%nbbills.txt >>%TEMP%nbbills.chk
      IF EXIST %PUB%NBDEPOS2 move %PUB%NBDEPOS2 %BURN%NBDEPOS2
      echo moved the %PUB%NBDEPOS2 file to %BURN%NBDEPOS2 >%TEMP%nbbills.log
		ppsftp BILL_NBDEPOS2_FILE_NOT_FOUND
      GOTO :EOF
:FTPFAILED
      ECHO FTP Failed >%TEMP%nbbills.chk
      TYPE %TEMP%nbbillsftp.log >>%TEMP%nbbills.chk
      IF EXIST %PUB%NBDEPOS2 move %PUB%NBDEPOS2 %BURN%NBDEPOS2
      echo moved the %PUB%NBDEPOS2 file to %BURN%NBDEPOS2 >%TEMP%nbbills.log
		ppsftp BILL_SFTP_FAILED
      GOTO :EOF