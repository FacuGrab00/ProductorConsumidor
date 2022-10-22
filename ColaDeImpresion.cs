using System;
using System.Collections.Generic;
using System.Text;

namespace ProductorConsumidor
{
    //Esta clase se encarga de manejar el recurso critico (en este caso particular: la cola de impresión)
    public class ColaDeImpresion
    {
        readonly static int max = 5;
        string[] queue = new string[max];
        int frente = -1;
        int final = -1;

        //Getters
        public int Final => final;
        public int Frente => frente;

        //Insertar elemento en la cola
        public void Enqueue(string dato)
        {
            //Caso desbordamiento
            if (final == (max - 1) && frente == 0 || final + 1 == frente)
                throw new Exception();
            else
            {
                //Incrementamos el final
                if (final == (max - 1))
                    final = 0;
                else
                    final += 1;

                //Ingresamos el dato a la cola
                queue[final] = dato;
                if (frente == -1)
                    frente = 0;
            }
        }

        //Eliminar elemento de la cola
        public string Dequeue()
        {
            string dato = "";
            if (frente == -1) //Cola vacia
                throw new Exception();
            else
            {
                dato = queue[frente];
                //Hay un solo elemento
                if (frente == final)
                {
                    frente = -1;
                    final = -1;
                }
                else
                {
                    if (frente == (max - 1))
                        frente = 0;
                    else
                    {
                        frente += 1;
                    }
                }
            }
            return dato;
        }
    }
}
