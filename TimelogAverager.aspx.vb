Imports System.IO
Imports System.Web.UI.DataVisualization.Charting
Imports System.Drawing

Partial Class TimelogAverager
    Inherits System.Web.UI.Page

    Dim dates As ArrayList = New ArrayList
    Dim totalDates As ArrayList = New ArrayList
    Dim mydates As DateProcessor = New DateProcessor
    Dim minInstances As Int64

    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim defaultTime As DateTime = New DateTime(2000, 1, 1, 0, 0, 0, 0)
        Dim instances As Int64, minInstances = -1

        'Initialise the date variables, or retrieve them from session variables if they already exist
        If Session("dates") Is Nothing Then
            dates = New ArrayList
        Else
            dates = Session("dates")
        End If
        If Session("totalDates") Is Nothing Then
            totalDates = New ArrayList
        Else
            totalDates = Session("totalDates")
        End If

        Dim uploadedFiles As HttpFileCollection = Request.Files
        Span1.InnerHtml = String.Empty

        If uploadedFiles.Count = 0 Then
            Span1.InnerHtml &= "Error:" & vbCrLf & "You must select a file first"
            Exit Sub
        End If

        For i As Integer = 0 To uploadedFiles.Count - 1
            Dim userPostedFile As HttpPostedFile = uploadedFiles(i)
            instances = 0

            Try
                If (userPostedFile.ContentLength > 0) Then
                    Span1.InnerHtml &= "<u>File #" & (i + 1) & "</u><br>"
                    Span1.InnerHtml &= "File Content Type: " & userPostedFile.ContentType & "<br>"
                    Span1.InnerHtml &= "File Size: " & userPostedFile.ContentLength & "kb<br>"
                    Span1.InnerHtml &= "File Name: " & userPostedFile.FileName & "<br>"

                    'Open the file
                    Try
                        ' Open the file using a stream reader
                        Dim sr As StreamReader = New StreamReader(userPostedFile.InputStream)
                        Dim myString As String = ""
                        Do While sr.Peek() >= 0
                            ' Read the stream to a string
                            Dim line As String = sr.ReadLine() & " "
                            Dim d As DateTime = mydates.getDate(line)
                            If d.Ticks > 0 Then
                                instances += 1
                                Dim timeSince As Double = mydates.msTimeSubtraction(d, defaultTime)
                                If (dates.Count < instances) Then
                                    dates.Add(timeSince)
                                    totalDates.Add(1)
                                Else
                                    dates(instances - 1) += timeSince
                                    totalDates(instances - 1) += 1
                                End If
                            End If
                        Loop  ' End of file parsing loop
                        If minInstances = -1 Then
                            minInstances = instances
                        Else
                            minInstances = Math.Min(minInstances, instances)
                        End If
                        sr.Close()
                    Catch ex As Exception
                        Span1.InnerHtml &= "Error:" & vbCrLf & ex.Message
                        Exit Sub
                    End Try
                End If
            Catch ex As Exception
                Span1.InnerHtml &= "Error:" & vbCrLf & ex.Message
                Exit Sub
            End Try

        Next i

        'Check to make sure that timestamps were found in the files
        If minInstances <= 0 Then
            Span1.InnerHtml &= "Error:" & vbCrLf & "No timestamps located in selected file"
            If uploadedFiles.Count > 1 Then
                Span1.InnerHtml &= "s"
            End If
            Me.Button_Download.Enabled = False
            Exit Sub
        End If

        'Clear chart series
        While (Chart1.Series.Count > 0)
            Chart1.Series.RemoveAt(0)
        End While

        'Set up a new array of points
        Me.Chart1.Series.Add("Series1")
        Me.Chart1.Series("Series1").ChartType = SeriesChartType.Point
        Me.Chart1.Series("Series1").BorderWidth = 4
        Me.Chart1.Series("Series1").Color = Drawing.Color.MediumVioletRed

        Dim firstXPos As Int64 = -1

        For y = 0 To minInstances - 1
            Dim d As Double = dates(y) / totalDates(y)
            Dim newTime As DateTime = New DateTime(2000, 1, 1, 0, 0, 0, 0).AddMilliseconds(d)

            If y < 20 Then
                Span1.InnerHtml &= "<br>" & newTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt")
            End If

            Dim xPos As Int64 = Int(d / 1000) 'Time is measured in milliseconds, so dividing by 1000 gives seconds
            If y = 0 Then
                firstXPos = xPos
            End If

            Chart1.Series("Series1").Points.AddXY(xPos - firstXPos, y)

        Next y
        Span1.InnerHtml &= "<br>..."

        'Set chart axis options
        Me.Chart1.ChartAreas(0).AxisX.Minimum = 0
        Me.Chart1.ChartAreas(0).AxisX.Title = "Time (secs)"
        Me.Chart1.ChartAreas(0).AxisX.TextOrientation = TextOrientation.Horizontal

        Me.Chart1.ChartAreas(0).AxisY.Title = "Point events"
        Me.Chart1.ChartAreas(0).AxisY.TextOrientation = TextOrientation.Rotated270

        'Store the dates as session  variables
        Session("dates") = dates
        Session("totalDates") = totalDates
        Session("minInstances") = minInstances

        'Enable download button
        Me.Button_Download.Enabled = True

    End Sub


    Protected Sub Button_Download_Click(sender As Object, e As EventArgs) Handles Button_Download.Click

        dates = Session("dates")
        totalDates = Session("totalDates")
        minInstances = Session("minInstances")

        If minInstances <= 0 Then
            Span1.InnerHtml &= "Error:" & vbCrLf & "Nothing to download"
            Exit Sub
        End If

        'Get response, set content type.
        Dim response As HttpResponse = HttpContext.Current.Response
        Dim fileName As String = String.Format("averagedReferences-{0}.log", String.Format("{0:yyyyMMdd}", DateTime.Today))
        response.ContentType = "text/csv"
        response.AddHeader("Content-Disposition", "attachment;filename=" & fileName)
        response.Clear()

        'Write time data to the response
        Dim writer As StreamWriter = New StreamWriter(response.OutputStream)
        For i = 0 To minInstances - 1
            Dim newTime As DateTime = New DateTime(2000, 1, 1, 0, 0, 0, 0).AddMilliseconds(dates(i) / totalDates(i))
            Dim mystring As String = newTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt")
            writer.WriteLine(mystring)
        Next i

        'Flush writer and finish response to end the downloading of the file
        writer.Flush()
        response.End()

    End Sub

End Class