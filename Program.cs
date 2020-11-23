using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

            string path = Directory.GetCurrentDirectory() + "\\Entradas\\entradaATM1.txt";

            grafo = Parser(path);

            List<Estado> resultado = SAT(verificacao);

            if(resultado == null)
            {
                Console.WriteLine("Vazio");
            }
            else
            {
                Console.WriteLine("Verificacao: " + verificacao);
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
            else if (verificacao.StartsWith("EX"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "EX", "");
                List<Estado> retorno = SATex(verificacao);
                return retorno;
            }
            else if (verificacao.StartsWith("AX"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "AX", "EX~");
                return SAT("~" + verificacao);
            }
            else if (verificacao.StartsWith("EF"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "EF", "");
                verificacao = "ETU" + verificacao;
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("EG"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "EG", "");
                verificacao = "~AF~" + verificacao;
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("AF"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "AF", "");
                return SATaf(verificacao);
            }
            else if (verificacao.StartsWith("AG"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "AG", "");
                verificacao = "~EF" + verificacao;
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("A"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "A", "");
                string[] split = verificacao.Split("U");
                verificacao = "~E~" + split[1] + "U~" + split[0] + "&~" + split[1] + "|EG~" + split[1];
                return SAT(verificacao);
            }
            else if (verificacao.StartsWith("E"))
            {
                verificacao = ReplaceFirstOccurrance(verificacao, "E", "");
                string[] split = verificacao.Split("U");
                return SATeu(split[0], split[1]);
            }
            else if (verificacao.Contains("&"))
            {
                string[] newVerificacao = verificacao.Split("&");
                List<Estado> lista1 = SAT(newVerificacao[0]);
                List<Estado> lista2 = SAT(newVerificacao[1]);
                List<Estado> retorno = new List<Estado>();

                retorno = lista1.Intersect(lista2).ToList();

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
                return SAT("~" + verificacao.Replace("-", "&"));
            }
            else if (verificacao.StartsWith("~"))
            {
                List<Estado> remove = SAT(ReplaceFirstOccurrance(verificacao, "~", ""));
                List<Estado> result = new List<Estado>();

                foreach (Estado aux in grafo.Estados)
                {
                    result.Add(aux);
                }
                foreach (Estado estado in remove.ToList())
                {
                    result.Remove(estado);
                }

                return result;
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
                    if(X.Contains(aux) && !Y.Contains(estado))
                    {
                        Y.Add(estado);
                    }
                }
            }

            return Y;
        }

        //Verificar SATAF
        //Verificar EGmon
        public static List<Estado> SATaf(string a)
        {

            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();
            List<Estado> Aux = new List<Estado>();

            X = grafo.Estados;
            Y = SAT(a);

            while (isNotEquals(X, Y))
            {
                X = Y;
                foreach (Estado estado in grafo.Estados)
                {
                    List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado.Nome)).ToList();
                    bool has = true;
                    foreach (Transicao transicao in transicoes)
                    {
                        Estado aux = grafo.Estados.Where(p => p.Nome.Equals(transicao.To)).FirstOrDefault();
                        if (!Y.Contains(aux))
                        {
                            has = false;
                        }
                    }
                    if (has && !Aux.Contains(estado))
                    {
                        Aux.Add(estado);
                    }
                }
                Y = Y.Union(Aux).ToList();
            }

            return Y;
        }

        public static List<Estado> SATeu(string a, string b)
        {
            List<Estado> X = new List<Estado>();
            List<Estado> Y = new List<Estado>();
            List<Estado> W = new List<Estado>();
            List<Estado> ListAux = new List<Estado>();
            List<Estado> Intersec = new List<Estado>();
            List<Estado> NewY = new List<Estado>();

            W = SAT(a);
            X = grafo.Estados;
            Y = SAT(b);

            while(isNotEquals(X,Y))
            {
                X = Y;
                foreach(Estado estado in grafo.Estados)
                {
                    List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado.Nome)).ToList();
                    foreach(Transicao transicao in transicoes)
                    {
                        Estado aux = grafo.Estados.Where(p => p.Nome.Equals(transicao.To)).FirstOrDefault();
                        if(Y.Contains(aux) && !ListAux.Contains(estado))
                        {
                            ListAux.Add(estado);
                        }
                    }
                }

                Intersec = W.Intersect(ListAux).ToList();
                NewY = Y.Union(Intersec).ToList();

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
            
            while(isNotEquals(X, Y))
            {
                X = Y;
                foreach(Estado estado in grafo.Estados)
                {
                    List<Transicao> transicoes = grafo.Transicoes.Where(p => p.From.Equals(estado)).ToList();
                    foreach(Transicao trans in transicoes)
                    {
                        Estado estadoAux = grafo.Estados.Where(p => p.Nome.Equals(trans.To)).FirstOrDefault();
                        if (Y.Contains(estadoAux) && !aux.Contains(estado))
                        {
                            aux.Add(estado);
                        }
                    }
                }


                Y = Y.Intersect(aux).ToList();
            }

            return Y;
        }


        public static bool isNotEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return !cnt.Values.All(c => c == 0);
        }

        public static string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            int loc = original.IndexOf(oldValue);
            if (loc == -1)
                return original;
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }
    }
}
