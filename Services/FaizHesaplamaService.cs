using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using FaizHesaplamaAPI.Data;

namespace FaizHesaplamaAPI.Services
{
    public class FaizHesaplamaService
    {
        private readonly AppDbContext _context;

        public FaizHesaplamaService(AppDbContext context)
        {
            _context = context;
        }

        public (decimal ToplamFaiz, decimal OrtalamaFaizOrani) HesaplaFaiz(DateOnly ilkTarih, DateOnly sonTarih, string bsmv)
        {
            //if (!decimal.TryParse(bsmv , NumberStyles.Any , CultureInfo.InvariantCulture, out decimal bsmvDecimal  )) //Global
            if (!decimal.TryParse(bsmv, NumberStyles.Any, new CultureInfo("tr-TR"), out decimal bsmvDecimal))       // TR'ye göre , ondalıklı sayılar "," ile  , globalde ise "." ile
            {
                throw new ArgumentException("Gecersiz ana para formati");
            }

            var periods = _context.Process
                .Where(p => p.ilkTarih <= sonTarih && (p.sonTarih >= ilkTarih || p.sonTarih == null))
                .OrderBy(p => p.ilkTarih)
                .ToList();


            decimal totalInterest = 0;
            decimal totalDayInterest = 0;
            int totalDay = 0;
            bool isLastPeriod = false;

            for (int i = 0; i < periods.Count; i++)
            {
                isLastPeriod = (i == periods.Count - 1); //Son donem kontrolu

                var period = periods[i];
                var startingPeriod = period.ilkTarih < ilkTarih ? ilkTarih : period.ilkTarih;
                var endingPeriod = period.sonTarih > sonTarih || period.sonTarih == null ? sonTarih : period.sonTarih;

                if (decimal.TryParse(period.faizOrani.Replace("%", ""), out decimal faizOrani))
                {
                    faizOrani /= 100;

                    //Son döneme +1 eklemeden diğerlerine ekleme:
                    int days = endingPeriod.Value.DayNumber - startingPeriod.DayNumber; //+ 1;
                    if (!isLastPeriod) days += 1;

                    decimal periodInterest = bsmvDecimal * faizOrani * days / 360;
                    periodInterest = Math.Round(periodInterest, 2);
                    totalInterest += periodInterest;

                    totalDayInterest += faizOrani * days;
                    totalDay += days;
                }

            }

            decimal ortalamaFaizOrani = totalDay > 0 ? (totalDayInterest / totalDay) * 100 : 0;
            return (ToplamFaiz: Math.Round(totalInterest, 2), OrtalamaFaizOrani: Math.Round(ortalamaFaizOrani, 2));

            // foreach (var period in periods)
            // {
            //     var startingPeriod = period.ilkTarih < ilkTarih ? ilkTarih : period.ilkTarih;
            //     var endingPeriod = period.sonTarih > sonTarih || period.sonTarih == null ? sonTarih : period.sonTarih;

            //     if (decimal.TryParse(period.faizOrani.Replace("%" , ""), out decimal faizOrani))
            //     {
            //         faizOrani /= 100;
            //         //int days = CalculateExactExcelDays(startingPeriod, endingPeriod.Value);
            //         //int days = (endingPeriod - startingPeriod).Days + 1; -> ilkTarih ve sonTarih DateOnly olması sebebi ile aralarında cıkarma islemi yapılamıyor.
            //         int days = endingPeriod.Value.DayNumber - startingPeriod.DayNumber +1; 
            //         decimal periodInterest = bsmvDecimal * faizOrani * days / 360;
            //         periodInterest = Math.Round(periodInterest, 2); // Faiz tutarını iki ondalık basamağa yuvarla
            //         totalInterest += periodInterest;
            //         //totalInterest = Math.Round(totalInterest , 2);

            //         //Ortalama faiz:
            //           totalDayInterest += faizOrani * days;
            //           totalDay += days;

            //     }

            // }

            // decimal ortalamaFaizOrani = totalDay > 0 ? (totalDayInterest / totalDay) * 100 : 0;
            // return (ToplamFaiz: Math.Round(totalInterest, 2), OrtalamaFaizOrani: Math.Round(ortalamaFaizOrani, 2));
            //// return (ToplamFaiz: totalInterest, OrtalamaFaizOrani: ortalamaFaizOrani);


        }

        //private int CalculateExactExcelDays(DateOnly start , DateOnly end)
        //{
        //    // Excel'in tam olarak kullandığı yöntem:
        //    // Başlangıç ve bitiş günlerini DateTime'a çevirip farkını alıyoruz
        //    DateTime startDt = new DateTime(start.Year, start.Month, start.Day);
        //    DateTime endDt = new DateTime(end.Year, end.Month, end.Day);
        //    return (endDt - startDt).Days + 1; // +1 ekliyoruz çünkü Excel son günü dahil ediyor
        //}




    }
}