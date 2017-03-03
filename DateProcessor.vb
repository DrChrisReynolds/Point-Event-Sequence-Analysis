Imports Microsoft.VisualBasic

Public Class DateProcessor

    'Initialise Date-Time Regular expression
    Dim yyyy As String = "(20\d\d|[^\d]\d\d)"   'two digit year (not preceded by a number), or year 2000-2099
    Dim MM As String = "(0?[1-9]|1[012])" '1-12
    Dim dd As String = "(0[1-9]|[12][0-9]|3[01])" '1-31
    Dim hhmmssuuu As String = "(0?[0-9]|1[0-9]|2[0-3])[\:\.]([0-5][0-9])[\:\.]([0-5][0-9])([\:\.\,](\d{1,3}))?"
    Dim AMPM As String = "(\s?([AaPp][Mm]))?"   'AM or PM
    'yyyy/MM/dd hh:mm:ss:uuu AM/PM
    Dim dateTimeValidator As String = yyyy & "[\.\-\/|]" & MM & "[\.\-\/|]" & dd & "[|Tt\s]" & hhmmssuuu & AMPM & "(\W+.*)?$"
    Dim regex As Regex = New Regex(dateTimeValidator)

    Function msTimeSubtraction(d1 As DateTime, d2 As DateTime) As Double
        Return d1.Subtract(d2).TotalMilliseconds
    End Function

    Function getDate(s As String) As DateTime

        Dim match As Match = regex.Match(s & " ")
        If match.Success Then
            Dim d As DateTime
            Dim regexYear As Integer, regexMonth As Integer, regexDay As Integer, regexHour As Integer, regexMin As Integer, regexSec As Integer, regexMs As Integer = 0
            regexYear = Integer.Parse(match.Groups(1).Value)
            If regexYear > -1 And regexYear < 100 Then
                regexYear += 2000
            End If
            regexMonth = Integer.Parse(match.Groups(2).Value)
            regexDay = Integer.Parse(match.Groups(3).Value)
            regexHour = Integer.Parse(match.Groups(4).Value)
            regexMin = Integer.Parse(match.Groups(5).Value)
            regexSec = Integer.Parse(match.Groups(6).Value)
            If (match.Groups(8).Success) Then
                regexMs = Integer.Parse(match.Groups(8).Value)
            End If
            If (match.Groups(10).Success And match.Groups(10).Value.ToUpper = "PM") Then
                regexHour = (regexHour Mod 12) + 12
            End If
            d = New DateTime(regexYear, regexMonth, regexDay, regexHour, regexMin, regexSec, regexMs)
            Return d
        Else
            Return Nothing
        End If

    End Function

End Class
