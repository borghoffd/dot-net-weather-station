/************************************* 
Fertigstellung:   13.01.22
Bearbeiter(in) 1:  Borghoff, Dennis
***************************************/ 

/************************************* 
Statement zu den Warnungen:
https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only
https://docs.microsoft.com/de-de/dotnet/fundamentals/code-analysis/quality-rules/ca1416
https://github.com/dotnet/sdk/issues/14502
https://stackoverflow.com/questions/65165941/what-is-the-proper-way-to-handle-error-ca1416-for-net-core-builds
https://stackoverflow.com/questions/69936093/how-to-fix-visual-studio-2022-warning-ca1416-call-site-reachable-by-all-platfor

***************************************/ 
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace bwi4032 {
    class Hausarbeit {
        /******************** 
        Konfig/Variablen für
        Datenquelle laden + CSV speichern 
        ********************/
        string filenameInfo = @"info-borghoff.jpg";
        string filenameEin = @"ein-borghoff.csv";
        string filenameAus = @"aus-borghoff.csv";

        int cntData = 0; // Anzahl der Datensätze, die aus dem Internet geladen. Wird zur dynamischen Berechnung der Länge zwischen den Koordinaten auf der X-Achse benutzt

        List<string> listNr = new List<string>(); // Liste der geladenen Werte, beinhaltet die Werte für die Nummer
        List<string> listDateTime = new List<string>(); // Liste der geladenen Werte, beinhaltet die Werte für die Zeit
        List<string> listDateTimeEdited = new List<string>(); // Liste, in der die Uhrzeit vom Datum getrennt ist
        List<string> listTemp = new List<string>(); // Liste der geladenen Werte, beinhaltet die Werte für die Temperatur

        public void loadDataAndcCreateCSV() {
            /******************** 
            Aktuelle Daten aus einer Datenquelle aus dem Internet laden. 
            ********************/
            using (WebClient webClient = new WebClient()) {
                webClient.Headers["Content-Type"] = "application/json";
                webClient.Headers["user-agent"] = "Mozilla/4.0 (compatible; MSIE 6.0; " + "Windows NT 5.2; .NET CLR 1.0.3705;)";

                Console.WriteLine("Beginne Daten herunterzuladen...");
                // In diesem Fall wird eine CSV-Datei aus dem Internet geladen
                using (Stream data = webClient.OpenRead("https://.../getKlimaWS.php")) {
                    using (StreamReader reader = new StreamReader(data)) {
                        using (StreamWriter writetext = new StreamWriter(filenameEin)) {
                            // Einmal durch die aus dem Internet geladene CSV Zeile für Zeile durchgehen
                            // Siehe https://stackoverflow.com/questions/5282999/reading-csv-file-and-storing-values-into-an-array
                            // Um nicht mehrmals durch die CSV zu gehen, werden gleichzeitig die Daten gespeichert und die gewünschten Werte extrahiert
                            while (!reader.EndOfStream) {
                                /******************** 
                                Die geladenen Daten abspeichern. 
                                ********************/
                                var line = reader.ReadLine();
                                writetext.WriteLine(line);


                                /******************** 
                                Aus den Daten Werte extrahieren
                                Daten: Temperatur
                                *********************/
                                var values = line.Split(';');

                                listNr.Add(values[0]); // Extrahiere Nr
                                listDateTime.Add(values[2]); // Extrahiere Zeit
                                listTemp.Add(values[3]); // Extrahiere Temperatur
                            }
                        }
                        cntData = listNr.Count - 1;
                        Console.WriteLine("Es wurden " + cntData + " Datensätze heruntergeladen.");

                        Console.WriteLine("Die heruntergeladenen Datensätze wurden in die Datei " + filenameEin + " gespeichert.");

                        /******************** 
                        Die Ergebnis-Werte abspeichern
                        ********************/
                        using (StreamWriter writetext = new StreamWriter(filenameAus)) {
                            for (int i = 0; i < listNr.Count; i++) {
                                writetext.Write(listNr[i] + ";" + listDateTime[i] + ";" + listTemp[i] + "\r\n");
                            }
                            Console.WriteLine("Die verdichteten Datensätze wurden in die Datei " + filenameAus + " gespeichert.");
                        }
                    }
                }
            }
        }

        public void generateAndSaveImage() {
            /******************** 
            Aus den Ergebnis - Werten eine(Informations -)Grafik erstellen.
            ********************/
            /******************** 
            Konfig/Variablen für
                Grafik erstellen
            ********************/
            // Eine Bitmap erzeugen, 768 Pixel breit, 768 Pixel hoch 
            Bitmap b = new Bitmap(768, 768);
            Graphics g = Graphics.FromImage(b);
            Pen WhitePen = new Pen(Color.White, 3);
            SolidBrush WhiteBrush = new SolidBrush(Color.White);
            FontFamily fontFam = new FontFamily("Arial");
            Font fontNormal = new Font(fontFam, 12, FontStyle.Regular, GraphicsUnit.Pixel);
            Font fontSmall = new Font(fontFam, 12, FontStyle.Regular, GraphicsUnit.Pixel);

            int xLeftMargin = 90; // Abstand von der linken Seite zur Y-Achse
            int yBottomMargin = 60; // Abstand von der unteren Seite zur X-Achse

            int markLength = 10; // Länge der Koordinatenstriche auf den Achsen

            int xAxisLength = b.Width - xLeftMargin; // Länge der X-Achse: Breite minus linker Abstand -> Die Achse soll bis zum Ende des Bildes gehen
            int xAxisY = b.Height - yBottomMargin; // Y-Startpunkt der X-Achse

            int yAxisLength = b.Height - yBottomMargin; // Länge der Y-Achse: Höhe minus unterer Abstand -> Die Achse soll bis zum Ende des Bildes gehen
            int yAxisX = b.Width - xLeftMargin; // X-Startpunkt der Y-Achse

            int stepSizeX = xAxisLength / cntData; // Länge des Abstands zwischen den Koordinaten
            int cntTempPercScala = 20; // Die Einteilung der Temperatur auf der Y-Achse, 20 bedeutet 20 Markierungen auf der Y-Achse, 10 bedeutet 10 Markierungen
            int oneStepCountYAxis = yAxisLength / 20; // 1er Schritte auf der Y-Achse
            int stepSizeY = yAxisLength / cntTempPercScala; // Schrittgröße auf der Y-Achse, basiert auf deren Länge und der Anzahl der gewollten Markierungen (cntTempPercScala)

            int recWidth = 10; // Es wird ein Rechteck statt einem Punkt für die Koordinaten-Werte gezeichnet, das ist die Breite davon

            List<int> XScalaValues = new List<int>(); // Liste, in der alle X-Koordinatenwerte der Punkte gespeichert werden
            List<int> YScalaValues = new List<int>(); // Liste, in der alle Y-Koordinatenwerte der Punkte gespeichert werden

            List<int[]> pointValues = new List<int[]>(); // Liste, in der X- und Y-Koordinatenwerte aller Punkte gespeichert werden

            // Für ein saubereres Zeichnen der Linien
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            Console.WriteLine("Beginne, die Info-Grafik zu erstellen...");

            #region X-Achse
            // Zeiche X-Achse
            g.DrawLine(WhitePen, xLeftMargin, xAxisY, b.Width, xAxisY);

            // Zeiche Markierungen auf der X-Achse
            for (int i = xLeftMargin + stepSizeX; i <= b.Width; i += stepSizeX) {
                g.DrawLine(WhitePen, i, xAxisY - (markLength / 2), i, xAxisY + (markLength / 2));
                XScalaValues.Add(i);
            }

            // Achsenbeschriftung
            g.DrawString("Zeit", fontNormal, WhiteBrush, 730, 730);

            // Koordinatenbeschriftung 
            // Hole NUR die Uhrzeit aus der Spalte 'Zeit' der .csv Datei
            // Da dieser Wert aber auch das Datum beinhaltet (z.B. '22.01.10 10:30') muss der Wert am Leerzeichen gesplitted werden und wird dann in eine neue Liste geschrieben
            bool first = true; // Zur Erkennung des ersten Werts in der foreach-Schleife
            foreach (var item in listDateTime) {
                // Der erste Eintrag in der List ist der Name der Spalte, nämlich 'Zeit'. Deswegen muss dieser Eintrag übersprungen werden
                if (first) {
                    first = false;
                    continue;
                }
                string[] test = item.Split(' ');
                listDateTimeEdited.Add(test[1]);
            }

            // Schreiben der Uhrzeit-Strings
            for (int i = 0; i < XScalaValues.Count; i++) {
                g.DrawString(listDateTimeEdited[i], fontSmall, WhiteBrush, XScalaValues[i] - 17, 768 - yBottomMargin + 8);
            }

            // Schreiben des Namens
            g.DrawString("Dennis Borghoff", fontNormal, WhiteBrush, 0, 747);
            #endregion X-Achse


            #region Y-Achse
            // Zeiche Y-Achse
            g.DrawLine(WhitePen, xLeftMargin, 0, xLeftMargin, b.Height - yBottomMargin);

            // Zeiche Markierungen auf der Y-Achse
            for (int i = xAxisY - stepSizeY; i > 0; i -= stepSizeY) {
                // Warum wird hier xLeftMargin benutzt und oben xAxisY? Was ist xAxisY? Was ist das Pendant für die Y-Achse, yAxisX?
                g.DrawLine(WhitePen, xLeftMargin - (markLength / 2), i, xLeftMargin + (markLength / 2), i);
                YScalaValues.Add(i);
            }

            // Achsenbeschriftung
            g.DrawString("Temperatur", fontNormal, WhiteBrush, 0, 15);
            g.DrawString("(in °C)", fontNormal, WhiteBrush, 25, 25);

            // Koordinatenbeschriftung
            int axisCnt = 1;
            foreach (var item in YScalaValues) {
                g.DrawString(axisCnt.ToString(), fontNormal, WhiteBrush, xLeftMargin - 30, item - 5);
                axisCnt += 1;
            }
            #endregion

            #region Zeichnen der Punkte im Koordinatensystem

            for (int i = 1; i < listNr.Count; i++) {
                // Berechnung der X-Positon: Linker Abstand plus XXXXX plus(dient zum zentrieren)
                int xValue = xLeftMargin + (stepSizeX * i) - recWidth / 2;

                // Umwandlung des Strings des Wertes der Temperatur in einen Double
                // Hier kann Missing Data auftreten
                double tempDoubleValue = Double.Parse(listTemp[i], CultureInfo.InvariantCulture); // CultureInfo.InvariantCulture bedeutet, dass hier am Punkt und nicht am Komma festgemacht wird, dass es sich um einen double Wert handelt             
                // Berechnung der Y-Positon: Die Werte sind hoch und GDI rechnet mit dem Punkt 0/0 in der linken oberen Ecke, deswegen müssen wir hier subtrahieren
                //Länge der Y-Achse minus XXXXX minus der Hälfte der Breite des Rechtecks (dient zum zentrieren)
                double yValueDouble = yAxisLength - (oneStepCountYAxis * tempDoubleValue) - recWidth / 2;
                // FillRectangle braucht einen Int wert, deswegen wird hier der Y-Wert von Double auf int umgerechnet.
                int yValueInt = Convert.ToInt32(yValueDouble);

                g.FillRectangle(WhiteBrush, xValue, yValueInt, recWidth, recWidth);

                // Speichere die X und Y-Werte in einer Liste, damit darüber iteriert werden kann
                int[] coordinatePointsValues = { xValue + recWidth / 2, yValueInt + recWidth / 2 }; // Da das Rechteck durch recWidth / 2 zentriert wurde, muss dieser Wert hier addiert werden. So erhält man den Mittelpunkt des Rechtecks
                pointValues.Add(coordinatePointsValues);
            }

            // Zeichne eine Linie zwischen den Punkten
            for (int j = 0; j < pointValues.Count - 1; j++) {
                g.DrawLine(WhitePen, pointValues[j][0], pointValues[j][1], pointValues[j + 1][0], pointValues[j + 1][1]);
            }
            #endregion

            /******************** 
            Die Grafik als Bild abspeichern
            ********************/
            b.Save(filenameInfo, ImageFormat.Jpeg);
            Console.WriteLine("Die Info-Grafik wurde fertig gestellt. Sie ist nun unter " + filenameInfo + " gespeichert.");
            Console.WriteLine("Das Programm is nun fertig.");
        }
    }

    class Program {
        static void Main(string[] args) {
            Hausarbeit ha = new Hausarbeit();
            ha.loadDataAndcCreateCSV();
            ha.generateAndSaveImage();
        }   
    }
}
