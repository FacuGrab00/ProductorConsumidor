using System;
using System.Collections.Generic;
using System.Text;

namespace ProductorConsumidor
{
    //La clase Random genera numeros aleatorios enteros.. pero el ejercicio me solicita numeros decimales.
    //La clase NumerosAleatorios se encarga de generar numeros aleatorios de tipo entero. (Adaptado al ejercicio, numero minimo generado es de 0,5)
    static public class NumerosAleatorios
    {
        
        static public double NumeroAleatorio()
        {
            int entero = new Random().Next(0, 4);
            int decima = 0;

            if (entero == 4)
                decima = new Random().Next(0, 5);
            else
            {
                if (entero == 0)
                    decima = new Random().Next(5, 9);
                else
                    decima = new Random().Next(0, 9);

            }

            return Convert.ToDouble($"{entero},{decima}");
        }

        static public double NumeroAleatorio(int max)
        {
            int entero = new Random().Next(0, max);
            int decima = 0;

            if (entero == 0)
                decima = new Random().Next(5, 9);
            else
                decima = new Random().Next(0, 9);

            return Convert.ToDouble($"{entero},{decima}");
        }
    }
}
