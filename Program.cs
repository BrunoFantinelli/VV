using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using VV.Estrutura;

namespace VV
{
    class Program
    {
        public static string verificacao;
        public static Grafo grafo = new Grafo();

        static void Main(string[] args)
        {

            string path = Directory.GetCurrentDirectory() + "\\Entradas\\entrada6.txt";

            grafo = Parser(path);

            List<Estado> resultado = SAT(verificacao);
            if(resultado == null)
            {
                Console.WriteLine("Vazio");
            }
            else
            {
                Console.WriteLine("Quantidade de estados: " + resultado.Count);
                Console.WriteLine("Estados: ");
                foreach(Estado estado in resultado)
                {
                    Console.WriteLine(estado.Nome);
                }
            }
        }

        public static Grafo Parser(string arquivo)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(arquivo);

            Grafo aux = new Grafo();

            aux.Estados = new List<Estado>();
            aux.Transicoes = new List<Transicao>();

            while ((line = file.ReadLine()) != null)
            {
                Estado estado = new Estado();
                if (line.StartsWith("T"))
                {
                    string[] divisao = line.Split(":");
                    string[] transicoes = divisao[1].Split(",");
                    for (int i = 0; i < transicoes.Length; i++)
                    {
                        Transicao transicao = new Transicao();
                        string[] splitFinal = transicoes[i].Split("-");
                        transicao.From = splitFinal[0];
                        transicao.To = splitFinal[1];
                        aux.Transicoes.Add(transicao);
                    }
                }
                else if (line.StartsWith("F"))
                {
                    string[] divisao = line.Split(":");
                    verificacao = divisao[1];
                }
                else
                {
                    string[] divisao = line.Split(",");
                    estado.Nome = divisao[0];
                    for (int i = 1; i < divisao.Length; i++)
                    {
                        estado.Rotulos.Add(divisao[i]);
                    }
                    aux.Estados.Add(estado);
                }
            }

            file.Close();
            return aux;
        }

        public static List<Estado> SAT(string verificacao)
        {
            if (verificacao.Equals("T"))
            {
                return grafo.Estados;
            }
            else if (verificacao.Equals("F"))
            {
                return null;
            }
            else if (Regex.IsMatch(verificacao, @"^[a-z]+$"))
            {
                return grafo.Estados.Where(p => p.Rotulos.Contains(verificacao)).ToList();
            }
            else if (verificacao.StartsWith("~"))
            {
                List<Estado> remove = SAT(verificacao.Replace("~", ""));
                List<Estado> result = grafo.Estados;
                foreach (Estado estado in remove)
                {
                    result.Remove(estado);
                }
                
                return result;
            }
            else if (verificacao.Contains("&"))
            {
                string[] newVerificacao = verificacao.Split("&");
                List<Estado> lista1 = SAT(newVerificacao[0]);
                List<Estado> lista2 = SAT(newVerificacao[1]);
                List<Estado> retorno = new List<Estado>();
                foreach(Estado estado1 in lista1)
                {
                    foreach(Estado estado2 in lista2)
                    {
                        if(estado1.Nome.Equals(estado2.Nome) && !retorno.Contains(estado1))
                        {
                            retorno.Add(estado1);
                        }
                    }
                }

                return retorno;
            }
            else if (verificacao.Contains("|"))
            {
                string[] newVerificacao = verificacao.Split("|");
                List<Estado> lista1 = SAT(newVerificacao[0]);
                List<Estado> lista2 = SAT(newVerificacao[1]);
                List<Estado> retorno = new List<Estado>();
                foreach (Estado estado1 in lista1)
                {
                    retorno.Add(estado1);
                }
                foreach (Estado estado2 in lista2)
                {
                    if (!retorno.Contains(estado2))
                    {
                        retorno.Add(estado2);
                    }
                }
                return retorno;
            }
            else if (verificacao.Contains("-"))
            {
                return SAT("~" + verificacao.Replace("-", "|"));
            }
            else if (verificacao.StartsWith("AX"))
            {
                verificacao = verificacao.Replace("AX", "EX~");
                return SAT("~" + verificacao);
            }
            else if (verificacao.StartsWith("EX"))
            {
                verificacao = verificacao.Replace("EX", "");
                List<Estado> retorno = SATex(verificacao);
                return retorno;
            }
            else if (verificacao.StartsWith("A"))
            {
                verificacao = verificacao.Replace("A", "");
                string[] split = verificacao.Split("U");
                verificacao = "~E~" + split[1] + "U~" + split[0] + "&~" + split[1] + "|EG~" + split[1];
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("E"))
            {
                verificacao = verificacao.Replace("E", "");
                string[] split = verificacao.Split("");
                return SATeu(split[0], split[1]);
            }
            else if (verificacao.StartsWith("EF"))
            {
                verificacao = verificacao.Replace("EF", "");
                verificacao = "ETU" + verificacao;
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("EG"))
            {
                verificacao = verificacao.Replace("EG", "");
                verificacao = "~AF~" + verificacao;
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("AF"))
            {
                verificacao = verificacao.Replace("AF", "");
                return SATaf(verificacao);
            }
            else if (verificacao.StartsWith("AG"))
            {
                verificacao = verificacao.Replace("AG", "");
                verificacao = "~EF" + verificacao;
                return SAT(verificacao);
            }

            return null;
        }


        public static List<Estado> SATex(string a)
        {
            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();
            List<Estado> estados = grafo.Estados;
            

            X = SAT(a);
            
            foreach(Estado estado in estados)
            {
                List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado.Nome)).ToList();
                foreach(Transicao trans in transicoes)
                {
                    Estado aux = estados.Where(p => p.Nome.Equals(trans.To)).FirstOrDefault();
                    if(X.Contains(aux) && !Y.Contains(aux))
                    {
                        Y.Add(estado);
                    }
                }
            }

            return Y;
        }
        
        public static List<Estado> SATaf(string a)
        {

            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();

            X = grafo.Estados;
            Y = SAT(a);

            while(X != Y)
            {
                X = Y;
                foreach(Estado estado in grafo.Estados)
                {
                    List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado.Nome)).ToList();
                    bool has = true;
                    foreach(Transicao transicao in transicoes)
                    {
                        Estado aux = grafo.Estados.Where(p => p.Nome.Equals(transicao.To)).FirstOrDefault();
                        if (!Y.Contains(aux))
                        {
                            has = false;
                        }
                    }
                    if (has && !Y.Contains(estado))
                    {
                        Y.Add(estado);
                    }
                }
            }

            return Y;
        }

        public static List<Estado> SATeu(string a, string b)
        {
            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();
            List<Estado> W = new List<Estado>();
            List<Estado> ListAux = new List<Estado>();
            List<Estado> Inter = new List<Estado>();
            List<Estado> NewY = new List<Estado>();

            W = SAT(a);
            X = grafo.Estados;
            Y = SAT(b);

            while(X != Y)
            {
                X = Y;
                foreach(Estado estado in grafo.Estados)
                {
                    List<Transicao> trans = grafo.Transicoes.Where(p => p.From.Equals(estado.Nome)).ToList();
                    foreach(Transicao transicao in trans)
                    {
                        Estado aux = grafo.Estados.Where(p => p.Nome == transicao.To).FirstOrDefault();
                        if(Y.Contains(aux) && !ListAux.Contains(aux))
                        {
                            ListAux.Add(aux);
                        }
                    }
                }
                foreach(Estado estado in ListAux)
                {
                    if (W.Contains(estado))
                    {
                        Inter.Add(estado);
                    }
                }

                foreach(Estado estado in Inter)
                {
                    if (Y.Contains(estado))
                    {
                        NewY.Add(estado);
                    }
                }
                Y = NewY;
            }

            return Y;
        }

        public static List<Estado> SATeg(string a)
        {
            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();
            List<Estado> aux = new List<Estado>();
            List<Estado> newY = new List<Estado>();

            Y = SAT(a);
            
            while(X != Y)
            {
                X = Y;
                foreach(Estado estado in grafo.Estados)
                {
                    List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado)).ToList();
                    foreach(Transicao trans in transicoes)
                    {
                        Estado estadoAux = grafo.Estados.Where(p => p.Nome.Equals(trans.To)).FirstOrDefault();
                        if (Y.Contains(estadoAux))
                        {
                            aux.Add(estado);
                        }
                    }
                }


                foreach (Estado estado in grafo.Estados)
                {
                    if(aux.Contains(estado) && Y.Contains(estado))
                    {
                        newY.Add(estado);
                    }
                }
                Y = newY;
            }

            return Y;
        }
    }
}
