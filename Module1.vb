Imports System.IO
Imports System.Net.Mail
Imports System.Text.RegularExpressions
Module Module1
   Dim PUB As String = ""
   Dim TEMP As String = ""
   Dim DestDirFile As String = ""
   Dim UserName As String = ""
   Dim Password As String = ""
   Dim DestServer As String = ""
   Dim LOCALFILE As String = ""
   Dim JobName As String = ""
   Dim aReport As New Text.StringBuilder
   Dim f As String = vbTab & "{0,-20}{1}"
   Sub Main()
      If My.Application.CommandLineArgs.Count = 7 Then
         SetVariables()
      ElseIf My.Application.CommandLineArgs.Count = 0 Then
         logline("The program needs 7 arguments in the following order: " & vbCrLf & _
                     "  1) PUB, 2) TEMP, 3) REMOTE_DIR/FILE 4) USERNAME, 5) PASSWORD, 6) SFTPHOST, 7) LOCALFILE")
         Environment.ExitCode = 100
         notify()
         Exit Sub
      ElseIf My.Application.CommandLineArgs.Count = 1 Then
         Dim tmparg As String = My.Application.CommandLineArgs.Item(0)
         logline(tmparg)
         Select Case tmparg.Substring(0, 3)
            Case "GET"
               JobName = "GetAuthFile"
            Case "BIL"
               JobName = "SendSettlementFile"
            Case "AUT"
               JobName = "SendAuthFile"
         End Select
         Environment.ExitCode = 100
         notify()
         Exit Sub
      Else
         Dim i As Integer = 1
         For Each s As String In My.Application.CommandLineArgs
            logline("Arg " & i & ": " & s)
            i += 1
         Next
         logline(vbCrLf)
         logline("The program needs 7 arguments in the following order: " & vbCrLf & _
                     "  1) PUB, 2) TEMP, 3) REMOTE_DIR/FILE, 4) USERNAME, 5) PASSWORD, 6) SFTPHOST, 7) LOCALFILE")
         Environment.ExitCode = 100
         notify()
         Exit Sub
      End If
      If JobName = "SendAuthFile" Then
         Send_File(".RCDMPPBA.TXT")
      ElseIf JobName = "SendSettlementFile" Then
         Send_File(".RCDMPPBS.TXT")
      ElseIf JobName = "GetAuthFile" Then
         Get_nbauthin()
      Else
         notify()
      End If
   End Sub
   Sub Send_File(ByVal FileName As String)
      DisplayVariables()
      Dim newfile As String = PUB & Now.ToString("yyyyMMddHHmm") & FileName
      Select Case JobName.Substring(0, 4)
         Case "Send"
            logline(String.Format(f, "Destination File:", newfile))
         Case Else
            logline(String.Format(f, "Bad Dest. File/Dir:", DestDirFile))
            logline("Program will end.")
            Environment.ExitCode = 100
            notify()
            Exit Sub
      End Select
      Using sw As New StreamWriter(newfile)
         For Each l As String In File.ReadAllLines(LOCALFILE)
            If Not l.StartsWith("/") Then
               If l.StartsWith("06?") Then
                  'sw.WriteLine(l.Replace(".", " "))
                  sw.WriteLine(UCase(Regex.Replace(l, "[^a-zA-Z0-9?]", " ")))
               Else
                  sw.WriteLine(l)
               End If
            End If
         Next
         sw.Flush()
      End Using
      Dim sftpcommands As String = TEMP & "sftp.txt"
      Using sw As New StreamWriter(sftpcommands)
         sw.WriteLine("cd " & DestDirFile)
         sw.WriteLine("put " & newfile)
         'sw.WriteLine("ls")
         sw.WriteLine("quit")
         sw.Flush()
      End Using
      Dim cmdline As String = "-i C:\ppkey\ppkey.ppk -P 1022 " & UserName & "@" & DestServer & " -batch -bc -be -b " & sftpcommands
      Dim p As New Process
      logline(vbCrLf & "psftp.exe " & cmdline)
      With p.StartInfo
         .FileName = "psftp.exe"
         .Arguments = cmdline
         .CreateNoWindow = True
         .UseShellExecute = False
         .RedirectStandardOutput = True
         .RedirectStandardError = True
      End With
      Try
         p.Start()
         'System.Threading.Thread.Sleep(6000)
         'p.WaitForExit(5000)
         logline(p.StandardError.ReadToEnd)
         logline(p.StandardOutput.ReadToEnd)
         logline("Exit Status: " & Environment.ExitCode)
         notify()
      Catch ex As Exception
         logline("SFTP ERROR: " & ex.Message)
         Environment.ExitCode = 100
         notify()
      End Try
   End Sub
   Function GetFileName() As String
      Dim rfiles As New List(Of String)
      Dim tmpResult As String = ""
      DisplayVariables()
      Dim sftpcommands As String = TEMP & "sftp.txt"
      Using sw As New StreamWriter(sftpcommands)
         sw.WriteLine("ls")
         sw.Flush()
      End Using
      Dim cmdline As String = "-i C:\ppkey\ppkey.ppk -P 1022 " & UserName & "@" & DestServer & " -batch -bc -be -b " & sftpcommands
      Dim p As New Process
      logline(vbCrLf & "psftp.exe " & cmdline)
      With p.StartInfo
         .FileName = "psftp.exe"
         .Arguments = cmdline
         .CreateNoWindow = True
         .UseShellExecute = False
         .RedirectStandardOutput = True
      End With
      Try
         p.Start()
         p.WaitForExit()
         Do While p.StandardOutput.Peek() >= 0
            Dim tmpline As String = p.StandardOutput.ReadLine()
            If tmpline.Contains(DestDirFile) Then
               Dim tmp As String = tmpline.Substring(tmpline.ToUpper.IndexOf(DestDirFile.ToUpper, 29))
               rfiles.Add(tmp)
               logline(tmpline)
            Else
               logline(tmpline)
            End If
         Loop
         logline("Exit Status: " & Environment.ExitCode)
      Catch ex As Exception
         logline("SFTP ERROR: " & ex.Message)
         Environment.ExitCode = 100
         logline("Exit Status: " & Environment.ExitCode)
         notify()
      End Try
      If rfiles.Count = 0 Then
         Return ""
         Environment.ExitCode = 100
         logline("There were no " & DestDirFile & " files on the server.")
         logline("Exit Status: " & Environment.ExitCode)
         notify()
      ElseIf rfiles.Count = 1 Then
         Return rfiles.Item(0)
      Else
         Dim newestfile As String = rfiles.Item(0)
         For i As Integer = 1 To rfiles.Count - 1
            If GetFileDate(rfiles.Item(i)).Ticks > GetFileDate(newestfile).Ticks Then
               newestfile = rfiles.Item(i)
            End If
         Next
         Return newestfile
      End If
   End Function
   Function GetFileDate(ByVal afile As String) As DateTime
      Dim year As String = afile.Substring(13, 4)
      Dim month As String = afile.Substring(9, 2)
      Dim day As String = afile.Substring(11, 2)
      Dim hour As String = afile.Substring(18, 2)
      Dim minute As String = afile.Substring(20, 2)
      Dim second As String = afile.Substring(22, 2)
      Dim tmpResult As DateTime = New DateTime(year, month, day, hour, minute, second)
      Return tmpResult
   End Function
   Sub Get_nbauthin()
      Dim FileToGet As String = GetFileName()
      If Not FileToGet.ToUpper.Contains(DestDirFile.ToUpper) Then
         logline(vbCrLf & "No " & DestDirFile & " file was found on the remote server.")
         Environment.ExitCode = 100
         logline(vbCrLf & "Exit Status: " & Environment.ExitCode)
         notify()
         Exit Sub
      End If
      logline(vbCrLf & String.Format(f, "File to Get:", FileToGet))
      Dim sftpcommands As String = TEMP & "sftp.txt"
      Using sw As New StreamWriter(sftpcommands)
         sw.WriteLine("get " & FileToGet & " " & LOCALFILE)
         sw.Flush()
      End Using
      Dim cmdline As String = "-i C:\ppkey\ppkey.ppk -P 1022 " & UserName & "@" & DestServer & " -batch -bc -be -b " & sftpcommands
      Dim p As New Process
      logline(vbCrLf & "psftp.exe " & cmdline)
      With p.StartInfo
         .FileName = "psftp.exe"
         .Arguments = cmdline
         .CreateNoWindow = True
         .UseShellExecute = False
         .RedirectStandardOutput = True
      End With
      Try
         p.Start()
         p.WaitForExit()
         logline(p.StandardOutput.ReadToEnd)
         logline("Exit Status: " & Environment.ExitCode)
         notify()
      Catch ex As Exception
         logline("SFTP ERROR: " & ex.Message)
         Environment.ExitCode = 100
         logline("Exit Status: " & Environment.ExitCode)
         notify()
      End Try
   End Sub
   Sub DisplayVariables()
      logline("Variables: ")
      logline(String.Format(f, "PUB:", PUB))
      logline(String.Format(f, "TEMP:", TEMP))
      logline(String.Format(f, "LOCALFILE:", LOCALFILE))
      logline(String.Format(f, "User Name:", UserName))
      logline(String.Format(f, "Password:", Password))
      logline(String.Format(f, "SFTP Server:", DestServer))
      logline(String.Format(f, "Job Name:", JobName))
   End Sub
   Sub SetVariables()
      PUB = My.Application.CommandLineArgs.Item(0).Trim
      TEMP = My.Application.CommandLineArgs.Item(1).Trim
      DestDirFile = My.Application.CommandLineArgs.Item(2).Trim
      UserName = My.Application.CommandLineArgs.Item(3).Trim
      Password = My.Application.CommandLineArgs.Item(4).Trim
      DestServer = My.Application.CommandLineArgs.Item(5).Trim
      LOCALFILE = My.Application.CommandLineArgs.Item(6).Trim
      If LOCALFILE.ToLower.Contains("nbauthot") Then
         JobName = "SendAuthFile"
         If Not File.Exists(LOCALFILE) Then
            logline("Unable to Find local file in path: " & LOCALFILE)
            Environment.ExitCode = 100
            logline("Exit Status: " & Environment.ExitCode)
            notify()
            Exit Sub
         End If
      ElseIf LOCALFILE.ToLower.Contains("nbdepos2") Then
         JobName = "SendSettlementFile"
         If Not File.Exists(LOCALFILE) Then
            logline("Unable to Find local file in path: " & LOCALFILE)
            Environment.ExitCode = 100
            logline("Exit Status: " & Environment.ExitCode)
            notify()
            Exit Sub
         End If
      ElseIf LOCALFILE.ToLower.Contains("nbauthin") Then
         JobName = "GetAuthFile"
      End If
   End Sub
   Sub notify()
      aReport.AppendLine(vbCrLf & Now.ToString)
      Dim aMessage As New MailMessage
      With aMessage
            .From = New MailAddress(My.Computer.Name & "@ecommerce.com")
            .To.Add("CCProcessReports@ecommerce.com")
            Select Case JobName
            Case "SendAuthFile"
               .Subject = "Send CC Authorization File Process Ran"
            Case "SendSettlementFile"
               .Subject = "Send CC Settlement File Process Ran"
            Case "GetAuthFile"
               .Subject = "Retrieve CC Authorization File Process Ran"
            Case Else
               .Subject = "Unknown CC sftp Process Ran: " & JobName
         End Select
         If Environment.ExitCode > 0 Then
            .Priority = MailPriority.High
            .Subject = .Subject & " -- ERROR!!!"
         End If
         .Body = aReport.ToString
      End With
      Dim SMTPClient As New SmtpClient("SMTP")
      Try
         SMTPClient.Send(aMessage)
      Catch ex As Exception
         Console.WriteLine(ex.Message)
      End Try
   End Sub
   Sub logline(ByVal aline As String)
      Console.WriteLine(aline)
      aReport.AppendLine(aline)
   End Sub
End Module
