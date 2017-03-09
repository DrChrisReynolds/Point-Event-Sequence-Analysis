Imports System.IO
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.DataVisualization.Charting
Imports System.Drawing

Partial Class ProcessMachineData
    Inherits System.Web.UI.Page

    Dim startTime As DateTime
    Dim sortedMessages As SortedList
    Dim mydates As DateProcessor = New DateProcessor

    Protected Sub Process_Button_Click(sender As Object, e As EventArgs) Handles Process_Button.Click

        'Handles the file for the comparison run
        Dim file1 As Boolean = FileUpload1.PostedFile IsNot Nothing And FileUpload1.PostedFile.ContentLength > 0
        Dim file2 As Boolean = FileUpload2.PostedFile IsNot Nothing And FileUpload2.PostedFile.ContentLength > 0

        If (file1 And file2) Then
            Error_Comparison()
        ElseIf (file1 And Not file2) Then
            SingleLineTrace()
        ElseIf (Not file1 And file2) Then
            Me.myspan.InnerHtml = "You must choose an event log to be compared against this reference log"
        Else
            Me.myspan.InnerHtml = "Please choose an event log (and optionally a reference log to compare it against)"
        End If

    End Sub

    Function readTimeLogFile(f As FileUpload, getStringData As Boolean) As List(Of DateTime)

        Dim a As List(Of DateTime) = New List(Of DateTime)

        'Initialise regular expressions
        Dim regex1 As Regex = New Regex("[^a-zA-Z0-9]{3,}"), regex2 As Regex = New Regex("\s+")

        'Handles the file for the comparison run
        If (f.PostedFile IsNot Nothing And f.PostedFile.ContentLength > 0) Then
            'Open the file
            Try
                ' Open the file using a stream reader.
                Dim sr As StreamReader = New StreamReader(f.FileContent)
                Dim myString As String = ""
                Do While sr.Peek() >= 0
                    ' Read the stream to a string
                    Dim line As String = sr.ReadLine() & " "
                    Dim d As DateTime = mydates.getDate(line)

                    If getStringData And d.Ticks > 0 Then
                        a.Add(d)

                        myString = regex1.Replace(myString.Trim, "...")
                        myString = regex2.Replace(myString, " ")
                        Dim dataList As ArrayList
                        If sortedMessages.ContainsKey(d) Then
                            dataList = sortedMessages(d)
                            dataList.Add(myString)
                        Else
                            dataList = New ArrayList
                            dataList.Add(myString)
                            sortedMessages.Add(d, dataList)
                        End If
                        myString = ""
                    ElseIf d.Ticks > 0 Then
                        a.Add(d)
                    ElseIf getStringData Then
                        myString &= line
                    End If
                Loop  ' End of file parsing loop
                sr.Close()
            Catch ex As Exception
                Me.myspan.InnerHtml = "Error:" & vbCrLf & ex.Message
            End Try
        End If

        Return a

    End Function

    Protected Sub SingleLineTrace()

        Dim a As List(Of DateTime) = New List(Of DateTime)
        sortedMessages = New SortedList

        a = readTimeLogFile(FileUpload1, True)

        Dim graph As SortedList(Of Int64, Int64) = New SortedList(Of Int64, Int64)
        graph.Add(0, 0)

        'Clear anything over 4 series
        While (Chart1.Series.Count > 4)
            Chart1.Series.RemoveAt(0)
        End While

        Dim seriesNumber As Integer = Chart1.Series.Count
        Dim seriesName = "Series" & (seriesNumber + 1)

        'Set up a new array of points
        Me.Chart1.Series.Add(seriesName)
        Me.Chart1.Series(seriesName).ChartType = SeriesChartType.Point
        Me.Chart1.Series(seriesName).BorderWidth = 4
        Me.Chart1.Series(seriesName).Color = {Drawing.Color.MediumVioletRed, Drawing.Color.ForestGreen, Drawing.Color.DarkBlue, Drawing.Color.DarkCyan}(seriesNumber)

        Dim aPointer As Integer = 0
        Dim prevTime As DateTime = a(0)
        While (aPointer < a.Count - 1)
            aPointer += 1
            Dim newtime As DateTime = a(aPointer)

            If newtime < prevTime Then Continue While

            Dim thisTime As Int64 = mydates.msTimeSubtraction(newtime, a(0))
            graph.Item(thisTime) = aPointer
            prevTime = newtime

        End While

        Dim lastX As Integer = -1,lastY As Integer = -1

        For Each entry As Long In graph.Keys
            Dim xPos As Int64 = Int(entry / 1000) 'Time is measured in milliseconds, so dividing by 1000 gives seconds
            Dim yPos As Int64 = graph(entry)

            If xPos = lastX And yPos = lastY Then Continue For

            Dim q As String = String.Format("{0}={1}", xPos, yPos)
            Chart1.Series(seriesName).Points.AddXY(xPos, yPos)

            lastX = xPos
            lastY = yPos

        Next entry

        'Store the session variables
        Session("sortedMessages") = sortedMessages

        'Register clientscript for returning the x-values of where the user has clicked on the graph
        Me.Chart1.Attributes("onclick") = ClientScript.GetPostBackEventReference(Me.Chart1, "@").Replace("'@'", "_getCoord(event)")
        'Set position to relative in order to get proper coordinates.
        Me.Chart1.Style(HtmlTextWriterStyle.Position) = "relative"
        'Register script
        Dim script As String = "function _getCoord(e){if(typeof(e.x)=='undefined')return e.layerX+','+e.layerY;return e.x+','+e.y}"
        Me.ClientScript.RegisterClientScriptBlock(Me.Chart1.GetType(), "Chart", script, True)

        'Set axis options
        Me.Chart1.ChartAreas(0).AxisX.Minimum = 0
        Me.Chart1.ChartAreas(0).AxisX.Title = "Time (secs)"
        Me.Chart1.ChartAreas(0).AxisX.TextOrientation = TextOrientation.Horizontal

        Me.Chart1.ChartAreas(0).AxisY.Title = "Point events"
        Me.Chart1.ChartAreas(0).AxisY.TextOrientation = TextOrientation.Rotated270

        'Set font style for the axes
        Dim axisFont As Font = New System.Drawing.Font("Arial", 13.0F, System.Drawing.FontStyle.Regular)
        Chart1.ChartAreas(0).AxisX.TitleFont = axisFont
        Chart1.ChartAreas(0).AxisY.TitleFont = axisFont

    End Sub




    Protected Sub Error_Comparison()

        Dim a As List(Of DateTime) = New List(Of DateTime)
        Dim b As List(Of DateTime) = New List(Of DateTime)
        sortedMessages = New SortedList

        a = readTimeLogFile(FileUpload1, True)
        b = readTimeLogFile(FileUpload2, False)

        'for each Datetime, calculate the area
        Dim difference As System.TimeSpan
        If DateTime.Compare(a(0), b(0)) < 0 Then
            startTime = a(0)
            difference = b(0) - startTime
        Else
            startTime = b(0)
            difference = a(0) - startTime
        End If

        'Synchronise the times so they both start at the same instant
        If DateTime.Compare(a(0), b(0)) < 0 Then
            For i = 0 To b.Count - 1
                b(i) = b(i).Subtract(difference)
            Next i
        Else
            For i = 0 To a.Count - 1
                a(i) = a(i).Subtract(difference)
            Next i
        End If

        'initial area is the distance between the two starting points
        Dim area As Double = 0

        Dim graph As SortedList(Of Int64, Int64) = New SortedList(Of Int64, Int64)
        graph.Add(0, 0)

        'Clear any previous series
        While (Chart1.Series.Count > 0)
            Chart1.Series.RemoveAt(0)
        End While
        'Set up a new array of points
        Me.Chart1.Series.Add("Series1")
        Me.Chart1.Series("Series1").ChartType = SeriesChartType.Point
        Me.Chart1.Series("Series1").BorderWidth = 4
        Me.Chart1.Series("Series1").Color = Drawing.Color.MediumVioletRed

        Dim aPointer As Integer = 0, bPointer As Integer = 0
        Dim prevTime As DateTime = startTime
        While (aPointer < a.Count - 1 And bPointer < b.Count - 1)
            Dim cmp As Integer, newtime As DateTime
            If (aPointer = a.Count - 1) Then
                cmp = 1
            ElseIf (bPointer = b.Count - 1) Then
                cmp = -1
            Else
                cmp = DateTime.Compare(a(aPointer + 1), b(bPointer + 1))
            End If
            Dim pointerDifference As Integer = aPointer - bPointer
            If cmp < 1 Then aPointer += 1
            If cmp > -1 Then bPointer += 1
            If (cmp < 1) Then
                newtime = a(aPointer)
            Else
                newtime = b(bPointer)
            End If

            If newtime < prevTime Then Continue While

            area += mydates.msTimeSubtraction(newtime, prevTime) * pointerDifference
            Dim thisTime As Int64 = mydates.msTimeSubtraction(newtime, startTime)
            graph.Item(thisTime) = area
            prevTime = newtime
        End While

        Dim lastX As Integer = -1,lastY As Integer = -1

        For Each entry As Long In graph.Keys

            Dim xPos As Int64 = Int(entry / 1000) 'Time is measured in milliseconds, so dividing by 1000 gives seconds
            Dim yPos As Int64 = Int(graph(entry) / 1000000)

            If xPos = lastX And yPos = lastY Then Continue For
            Dim q As String = String.Format("{0}={1}", xPos, yPos)
            Chart1.Series("Series1").Points.AddXY(xPos, yPos)

            lastX = xPos
            lastY = yPos

        Next entry


        'Store the session variables
        Session("sortedMessages") = sortedMessages
        Session("maximumX") = lastX

        'Register clientscript for returning the x-values of where the user has clicked on the graph
        Me.Chart1.Attributes("onclick") = ClientScript.GetPostBackEventReference(Me.Chart1, "@").Replace("'@'", "_getCoord(event)")
        'Set position to relative in order to get proper coordinates.
        Me.Chart1.Style(HtmlTextWriterStyle.Position) = "relative"
        'Register script
        Dim script As String = "function _getCoord(e){if(typeof(e.x)=='undefined')return e.layerX+','+e.layerY;return e.x+','+e.y}"
        Me.ClientScript.RegisterClientScriptBlock(Me.Chart1.GetType(), "Chart", script, True)

        'Set axis options
        Me.Chart1.ChartAreas(0).AxisX.Minimum = 0
        Me.Chart1.ChartAreas(0).AxisX.Title = "Time (secs)"
        Me.Chart1.ChartAreas(0).AxisX.TextOrientation = TextOrientation.Horizontal

        Me.Chart1.ChartAreas(0).AxisY.Title = "Cumulative error (secs)"
        Me.Chart1.ChartAreas(0).AxisY.TextOrientation = TextOrientation.Rotated270

        'Set font style for the axes
        Dim axisFont As Font = New System.Drawing.Font("Arial", 13.0F, System.Drawing.FontStyle.Regular)
        Chart1.ChartAreas(0).AxisX.TitleFont = axisFont
        Chart1.ChartAreas(0).AxisY.TitleFont = axisFont

    End Sub


    Protected Sub Chart1_Click(sender As Object, e As ImageMapEventArgs) Handles Chart1.Click

        'Initialise the support vector sentence classifier
        Dim sv As New SVMClassification
        sv.ScreenCompoundSVM()

        'Restore the session variables
        sortedMessages = Session("sortedMessages")
        startTime = sortedMessages.GetKey(0)    ' Session("comparisonStartTime")

        'Get the xy coordinates from the postback
        Dim Xpos As Double = Double.Parse(e.PostBackValue.Split(",")(0))
        Dim Ypos As Double = Double.Parse(e.PostBackValue.Split(",")(1))

        'Get the chart details from the session state
        Dim chartAreaXPixelStart As Double = Page.Session("chartAreaXPixelStart"), chartAreaXPixelEnd As Double = Page.Session("chartAreaXPixelEnd"), _
            xValueMin As Double = Page.Session("xValueMin"), xValueMax As Double = Page.Session("xValueMax"), _
            chartAreaYPixelStart As Double = Page.Session("chartAreaYPixelStart"), chartAreaYPixelEnd As Double = Page.Session("chartAreaYPixelEnd"), _
            yValueMin As Double = Page.Session("yValueMin"), yValueMax As Double = Page.Session("yValueMax")

        'Calculate the chart xy position from the xy coordinates of the click
        Xpos = xValueMin + (xValueMax - xValueMin) * ((Xpos - chartAreaXPixelStart) / (chartAreaXPixelEnd - chartAreaXPixelStart))
        Ypos = yValueMin + (yValueMax - yValueMin) * ((Ypos - chartAreaYPixelStart) / (chartAreaYPixelEnd - chartAreaYPixelStart))

        Dim targetDate As DateTime = startTime.AddSeconds(Xpos)
        Dim maxTimeDiff As Integer = Session("maximumX") * 0.05   '3600           ' the maximum time (in seconds) to search around the click area
        Dim maxTimeSpan As TimeSpan = TimeSpan.FromSeconds(maxTimeDiff)

        Dim predictedErrors As SortedList = New SortedList

        'Loop through the sorted messages
        For Each d As DateTime In sortedMessages.Keys
            If targetDate.Subtract(d) <= maxTimeSpan Then
                For Each x As String In sortedMessages(d)
                    Dim analysis As Double = sv.analyse(x)
                    If analysis > 1 Then analysis = 1
                    If analysis < 0 Then analysis = 0
                    'Calculate the colour hue to display the background colour in
                    Dim hue As Double = -1.66667 * analysis + 1.5833
                    hue = Math.Min(Math.Max(hue, 0), 0.75)
                    Dim rgbString As String = "hsl(" & Math.Round(255 * hue) & ",70%,70%)"
                    Dim myString As String = "<tr><td style='background-color:" & rgbString & "'>" & String.Format("{0:0.000}", analysis) & "</td><td>" & d.ToString & "</td><td>" & x & "</td></tr>"
                    If predictedErrors.ContainsKey(analysis) Then
                        predictedErrors(analysis) &= vbCrLf & myString
                    Else
                        predictedErrors.Add(analysis, myString)
                    End If
                Next x
            End If
        Next d

        Dim myString2 As String = ""
        For i = 1 To 10
            myString2 &= predictedErrors.GetByIndex(predictedErrors.Count - i) & "<br>"
        Next i
        Me.myspan.InnerHtml = "<table border=1 cellpadding=3><tr><th>Error probability</th><th>Timestamp</th><th>Error text</th></tr>" & myString2 & "</table>"

        Chart1.ChartAreas("MainChartArea").AxisX.StripLines(0).IntervalOffset = Xpos - (maxTimeDiff / 2)
        Chart1.ChartAreas("MainChartArea").AxisX.StripLines(0).StripWidth = maxTimeDiff
        Chart1.ChartAreas("MainChartArea").AxisX.StripLines(0).BackColor = Color.FromArgb(&H51FF0000)

    End Sub

    Protected Sub Chart1_PostPaint(sender As Object, e As ChartPaintEventArgs) Handles Chart1.PostPaint

        'Store chart position and pixel data in the session state
        Page.Session("xValueMin") = Chart1.ChartAreas("MainChartArea").AxisX.Minimum
        Page.Session("xValueMax") = Chart1.ChartAreas("MainChartArea").AxisX.Maximum
        Page.Session("yValueMin") = Chart1.ChartAreas("MainChartArea").AxisY.Minimum
        Page.Session("yValueMax") = Chart1.ChartAreas("MainChartArea").AxisY.Maximum

        Page.Session("chartAreaXPixelStart") = Chart1.ChartAreas("MainChartArea").AxisX.ValueToPixelPosition(Chart1.ChartAreas("MainChartArea").AxisX.Minimum)
        Page.Session("chartAreaXPixelEnd") = Chart1.ChartAreas("MainChartArea").AxisX.ValueToPixelPosition(Chart1.ChartAreas("MainChartArea").AxisX.Maximum)
        Page.Session("chartAreaYPixelStart") = Chart1.ChartAreas("MainChartArea").AxisY.ValueToPixelPosition(Chart1.ChartAreas("MainChartArea").AxisY.Minimum)
        Page.Session("chartAreaYPixelEnd") = Chart1.ChartAreas("MainChartArea").AxisY.ValueToPixelPosition(Chart1.ChartAreas("MainChartArea").AxisY.Maximum)

    End Sub

    Protected Sub UpdateButton_Click(sender As Object, e As EventArgs) Handles UpdateButton.Click
        Dim rangeValue As String = TextBox1.Text
        rangeValue = rangeValue.Replace(" ", "")
        Dim r As String() = rangeValue.Split("-")

        Dim minVal As Integer = 0, maxVal As Integer = 0

        If r.Length > 1 Then
            minVal = Integer.Parse(r(0))
            maxVal = Integer.Parse(r(1))
        Else
            maxVal = Integer.Parse(r(0))
        End If

        Chart1.ChartAreas("MainChartArea").AxisX.Minimum = minVal
        Chart1.ChartAreas("MainChartArea").AxisX.Maximum = maxVal

    End Sub

End Class
