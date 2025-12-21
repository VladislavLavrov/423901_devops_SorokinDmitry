using DocumentFormat.OpenXml.Drawing;

using SortRollWebApp.Models;
using SortRollWebApp.Models.Entities;

namespace SortRollWebApp.Services
{
    public class RollingCalculator
    {
        private readonly GlobalContext _context;

        public RollingCalculator(GlobalContext context)
        {
            _context = context;

        }

        public void ChoseInitFeatures()
        {

        }

        public void Domain() // Расчет по методу кафедры ОМД
        {
            ZAGLUSHKA();
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
                Raskat(i);
            if ((_context.GlobalRollingParameters.TS == 1) || (_context.GlobalRollingParameters.TS == 2)) // скоростной режим для непрерывного и последовательного
                Skorost(_context.GlobalRollingParameters.TS, _context.GlobalRollingParameters.K12); // станов
            else
                Skorost2(); // скоростной режим для полунепрерывного стана с петлевой группой
            Tempsig(); // расчет температурного режима и сопротивления деформации
            Ushirenie(); // расчет коэфф.уширения с учетом марки стали
            Ugoln(); // расчет допустимых углов захвата
            Ustoichn(); // расчет допустимых отношений осей
            PowerOMD(); // энергосиловые параметры по методу кафедры омд
                        // ---------------------------------------------------------------
            SkorostMax(); // определение максимально возможной скорости прокатки
            LastCorrection();
        }

        public void DomainSP() // Расчет по методу Соответственной полосы
        {
            ZAGLUSHKA();
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
                Raskat(i);
            if ((_context.GlobalRollingParameters.TS == 1) || (_context.GlobalRollingParameters.TS == 2)) // скоростной режим для непрерывного и последовательного
                Skorost(_context.GlobalRollingParameters.TS, _context.GlobalRollingParameters.K12); // станов
            else
                Skorost2(); // скоростной режим для полунепрерывного стана с петлевой группой
            Tempsig(); // расчет температурного режима и сопротивления деформации
            SpPolosa(); // расчет энергосиловых параметров по методу соответственной полосы
        }

        double Spred(double C0, double C1, double C2, double C3, double C4, double C5, double C6, double C7, double KOBJ, double A, double A0, double AK, double DEL0, double PSITR, double TANFI)
        {
            double AA = Math.Exp(C1 * Math.Log(KOBJ - 1));
            double BB = Math.Exp(C2 * Math.Log(A));
            double CC = Math.Exp(C3 * Math.Log(A0));
            double DD; if (AK < 0.001) DD = 1; else DD = Math.Exp(C4 * Math.Log(AK));
            double EE = Math.Exp(C5 * Math.Log(DEL0));
            double FF = Math.Exp(C6 * Math.Log(PSITR));
            double GG; if ((int)(TANFI * 1000) != 0) GG = Math.Exp(C7 * Math.Log(TANFI)); else GG = 1;
            return 1 + C0 * AA * BB * CC * DD * EE * FF * GG;
        }

        void LastCorrection() // Последняя корректировка перед выводом отчетов
        {
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                if (_context.GlobalRollingParameters.K17 == 1)// устранение ошибок в расчете заполнения - с расчетом формоизменения
                    pass.PassRollingParameters.DEL1 = pass.PassRollingParameters.DELRZ;
                else
                     if (pass.PassRollingParameters.DELOP > 0.1)
                    pass.PassRollingParameters.DEL1 = pass.PassRollingParameters.DELOP;
            }
        }

        // Метод для максимального задания условий расчетов
        public void ZAGLUSHKA()
        {
            _context.GlobalRollingParameters.K14 = 9;
            _context.GlobalRollingParameters.K11 = 3;

        }
        public int ParZeroData()// Расчет del1 для нулевого прохода
        {
            switch (_context.Passes[1].Pass.SchemaInput)
            {
                case 0://гладкая бочка
                case 1://Гладкая бочка 1
                case 2://Круг          2
                case 12://Квадрат      12
                    _context.Passes[0].PassRollingParameters.DEL1 = 1;
                    _context.Passes[1].PassRollingParameters.DEL0 = 1;
                    break;
                case 3://Квадрат       3
                case 6://Шестиугольник 6
                case 7://Ромб          7
                    _context.Passes[0].PassRollingParameters.DEL1 = 0.85;
                    _context.Passes[1].PassRollingParameters.DEL0 = 0.85;
                    break;
                case 4://Овал          4
                    _context.Passes[0].PassRollingParameters.DEL1 = 0.80;
                    _context.Passes[1].PassRollingParameters.DEL0 = 0.80;
                    break;
                case 5://Плоский овал  5
                case 10://Ребровой овал 10
                    _context.Passes[0].PassRollingParameters.DEL1 = 0.95;
                    _context.Passes[1].PassRollingParameters.DEL0 = 0.95;
                    break;
                case 8://Ящик          8
                case 9://Новый ящик    9
                    _context.Passes[0].PassRollingParameters.TANFI = 0.3; //0.2 новые данные
                    _context.Passes[0].PassRollingParameters.DEL1 = 0.90;
                    _context.Passes[1].PassRollingParameters.DEL0 = 0.90;
                    break;
                default: break;
            };

            _context.Passes[0].PassRollingParameters.BK = _context.Passes[1].Pass.H0 / _context.Passes[1].PassRollingParameters.DEL0;
            _context.Passes[0].Pass.B1 = _context.Passes[0].PassRollingParameters.BK;
            _context.Passes[0].PassRollingParameters.AK = _context.Passes[0].PassRollingParameters.BK / _context.Passes[1].Pass.B0;
            _context.Passes[0].Pass.W = _context.InitialParameters.W0;
            if (_context.Passes[1].Pass.SchemaInput == 4)
                _context.Passes[0].Pass.ROV = _context.Passes[1].Pass.B0 * (1 + _context.Passes[0].PassRollingParameters.AK * _context.Passes[0].PassRollingParameters.AK) / 4;
            if (_context.Passes[1].Pass.SchemaInput == 8)
            {
                _context.Passes[0].Pass.BD = _context.Passes[0].PassRollingParameters.BK - _context.Passes[0].PassRollingParameters.TANFI * _context.Passes[1].Pass.B0;
                _context.Passes[0].PassRollingParameters.DELV = (_context.Passes[1].Pass.H0 - _context.Passes[0].Pass.BD) / (_context.Passes[0].PassRollingParameters.BK - _context.Passes[0].Pass.BD);
            }
            //     case 11://Шестигранник  11
            return 0;
        }

        public int AnalisData(ref int NumMistake, ref int NumCol, ref int NumPr, int[] pTypeMistakes, int[] pColMistakes, int[] pKletMistakes, ref int KolMistakes) //проверка исходных данных
        {
            NumMistake = 0; NumCol = 0; NumPr = 0; KolMistakes = 0;
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                //проверка правильности назначения схемы прокатки
                switch (pass.Pass.SchemaCaliber)
                {
                    case 0://гладкая бочка
                    case 1:
                        pass.Pass.SchemaCaliber = 0;
                        if (pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 3 || pass.Pass.SchemaInput == 5 || pass.Pass.SchemaInput == 6 || pass.Pass.SchemaInput == 8 || pass.Pass.SchemaInput == 11
                            || pass.Pass.SchemaInput == 4 || pass.Pass.SchemaInput == 9)
                            pass.Pass.SchemaInput = 1;
                        if (!(pass.Pass.SchemaInput == 1 || pass.Pass.SchemaInput == 2))
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 2://круглый калибр
                        if (pass.Pass.SchemaInput == 6 || pass.Pass.SchemaInput == 9 || pass.Pass.SchemaInput == 7)
                            pass.Pass.SchemaInput = 4;
                        if (pass.Pass.SchemaInput == 1)
                            pass.Pass.SchemaInput = 5;
                        if (!(pass.Pass.SchemaInput == 4 || pass.Pass.SchemaInput == 5))
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 3://диагональный квадратный калибр (предполагается, что это квадратный калибр)
                        if (pass.Pass.SchemaInput == 9)
                            pass.Pass.SchemaInput = 4;
                        if (pass.Pass.SchemaInput == 10)
                            pass.Pass.SchemaInput = 6;
                        if (pass.Pass.SchemaInput == 3)
                            pass.Pass.SchemaInput = 7;
                        if (pass.Pass.SchemaInput != 4 && pass.Pass.SchemaInput != 6 && pass.Pass.SchemaInput != 7)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 4://овальный калибр
                        if (pass.Pass.SchemaInput == 3 || pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 5 || pass.Pass.SchemaInput == 8)
                            pass.Pass.SchemaInput = 1;
                        if (pass.Pass.SchemaInput != 1 && pass.Pass.SchemaInput != 2 && pass.Pass.SchemaInput != 9)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 5://плоский овальный калибр
                        if (pass.Pass.SchemaInput == 2 || pass.Pass.SchemaInput == 3 || pass.Pass.SchemaInput == 8 || pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 11)
                            pass.Pass.SchemaInput = 1;
                        if (pass.Pass.SchemaInput != 1)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 6://шестиугольный калибр
                        if (pass.Pass.SchemaInput == 3 || pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 11 || pass.Pass.SchemaInput == 10)
                            pass.Pass.SchemaInput = 1;
                        if (pass.Pass.SchemaInput != 1)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 7://ромбичекий калибр
                        if (pass.Pass.SchemaInput == 1 || pass.Pass.SchemaInput == 2 || pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 8 || pass.Pass.SchemaInput == 10)
                            pass.Pass.SchemaInput = 3;
                        if (pass.Pass.SchemaInput != 3 && pass.Pass.SchemaInput != 7)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 8: //ящичный калибр
                        if (pass.Pass.SchemaInput == 3 || pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 11)
                            pass.Pass.SchemaInput = 1;
                        if (pass.Pass.SchemaInput != 1 && pass.Pass.SchemaInput != 8)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 9: //ребровой овальный калибр
                        if (pass.Pass.SchemaInput == 6 || pass.Pass.SchemaInput == 7 || pass.Pass.SchemaInput == 5 || pass.Pass.SchemaInput == 1)
                            pass.Pass.SchemaInput = 4;
                        if (pass.Pass.SchemaInput != 4)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 11: //ребровой калибр
                        pass.Pass.SchemaCaliber = 0;
                        if (pass.Pass.SchemaInput == 12 || pass.Pass.SchemaInput == 8 || pass.Pass.SchemaInput == 5 || pass.Pass.SchemaInput == 4 || pass.Pass.SchemaInput == 6)
                            pass.Pass.SchemaInput = 1;
                        if (pass.Pass.SchemaInput != 1)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    case 10: //шестигранный калибр
                        pass.Pass.SchemaCaliber = 10;//БЫЛО 6
                        if (pass.Pass.SchemaInput == 8)
                            pass.Pass.SchemaInput = 6;
                        if (pass.Pass.SchemaInput != 6)
                        {
                            NumMistake = 1;//ошибка неправильно заданного калибра
                            NumCol = 2;
                            NumPr = i;
                            return 1;
                        }
                        break;
                    default:
                        NumMistake = 1;//ошибка неправильно заданного калибра
                        NumCol = 2;
                        NumPr = i;
                        return 1;
                }//окончание проверки условия по схеме
                 //Проверка наличия обжатия полосы
                if (pass.Pass.H0 < pass.Pass.H1 && _context.GlobalRollingParameters.K17 == 0)
                {
                    NumMistake = 2;//ошибка неправильно заданного высота подката
                    NumCol = 4;
                    NumPr = i;
                    return 1;
                }// Проверка наличия вытяжки полосы
                if (i == 1 && _context.GlobalRollingParameters.K17 == 0 && _context.GlobalRollingParameters.K14 == 0)
                {
                    if (_context.InitialParameters.W0 < pass.Pass.W)
                    {
                        NumMistake = 3;//ошибка неправильно задана площадь сечения
                        NumCol = 8;
                        NumPr = i;
                        return 1;
                    }
                }
                else if (i > 1) // Проверяем i > 1 перед доступом к i-1
                {
                    var prevPass = _context.Passes[i - 1];
                    if (prevPass.Pass.W < pass.Pass.W && _context.GlobalRollingParameters.K17 == 0 && _context.GlobalRollingParameters.K14 == 0)
                    {
                        NumMistake = 4;//ошибка неправильно задана площадь сечения
                        NumCol = 8;
                        NumPr = i;
                        return 1;
                    }
                } //////// ПРЕДУСМОТРЕТЬ ДВОЙНОЙ ВЫХОД     /////////////////////
                if (pass.Pass.B1 < pass.Pass.B0 && _context.GlobalRollingParameters.K17 == 0)//проверка правильности задания ширины полосы
                    if (KolMistakes < 100)
                    {
                        pTypeMistakes[KolMistakes] = 5;//ошибка неправильно заданного ширина подката
                        pColMistakes[KolMistakes] = 5;//||7  - B1,B0
                        pKletMistakes[KolMistakes] = i;
                        KolMistakes++;
                    }//ПРОВЕРКА ЗАПОЛНЕНИЯ КАЛИБРА
                if (pass.Pass.B1 > pass.Pass.BVR && _context.GlobalRollingParameters.K17 == 0)
                    if (KolMistakes < 100)
                    {
                        pTypeMistakes[KolMistakes] = 6;//ПЕРЕПОНЕНИЕ КАЛИБРА
                        pColMistakes[KolMistakes] = 7;//B1,BVR      11
                        pKletMistakes[KolMistakes] = i;
                        KolMistakes++;
                    }// ПРОВЕРКА СООТВЕТСТВИЯ ШИРИНЫ ВРЕЗА И РАДИУСА ОВАЛЬНОГО КАЛИБРА
                if (pass.Pass.SchemaCaliber == 4)
                {
                    double rr = (pass.Pass.H1 - pass.Pass.S) * (1 + (pass.Pass.BVR / (pass.Pass.H1 - pass.Pass.S)) * (pass.Pass.BVR / (pass.Pass.H1 - pass.Pass.S))) / 4;
                    if (rr + 0.001 < pass.Pass.ROV || rr > 0.001 + pass.Pass.ROV)//ошибка несоотв. заданного и расчетн. радиуса овала
                    {
                        pass.Pass.ROV = rr;//Несоответствие расчетного и заданного радиусов овального калибра в клети i
                        if (KolMistakes < 100)
                        {
                            pTypeMistakes[KolMistakes] = 7;
                            pColMistakes[KolMistakes] = 9;//R,BVR
                            pKletMistakes[KolMistakes] = i;
                            KolMistakes++;
                        }
                    }
                }
            }
            //for (int i = 1; i < _context.GlobalRollingParameters.NPR + 1; i++)
            //{
                //var pass = _context.Passes[i];
                //if (pass.Pass.SchemaCaliber > 9)
                //    pass.Pass.Schema = pass.Pass.SchemaInput * 100 + pass.Pass.SchemaCaliber;
                //else
                //    pass.Pass.Schema = pass.Pass.SchemaInput * 10 + pass.Pass.SchemaCaliber;
            //}
            //Проверка заданных параметров
            if (_context.GlobalRollingParameters.K12 == 1)
                for (int i = 1; i < _context.GlobalRollingParameters.NPR + 1; i++)
                {
                    var pass = _context.Passes[i];
                    if (pass.RollingStand.NZ < 1)
                    {
                        NumMistake = 8;//ошибка неправильно заданного калибра
                        NumCol = 2;
                        NumPr = i;
                        return 1;
                    }
                }

            if (((_context.GlobalRollingParameters.K11 == 1) || (_context.GlobalRollingParameters.K11 == 3)) && ((Math.Abs(_context.Steel.K1) < 0.01) || (Math.Abs(_context.Steel.K2) == 0) || (Math.Abs(_context.Steel.K3) == 0) || (Math.Abs(_context.Steel.K4) == 0)))
            {
                NumMistake = 9;//ошибка по Зюзину
                return 1;
            }
            if (_context.GlobalRollingParameters.K11 == 2 && ((_context.Steel.K5 == 0) || (_context.Steel.K6 == 0) || (_context.Steel.K7 == 0) || (_context.Steel.K8 == 0) || (_context.Steel.K9 == 0)))//по методу Тюленёва
            {
                NumMistake = 10;//по Тюленеву
                return 1;
            }
            return 0;
        }

        public void PowerOMD() // энергосиловые параметры по методу кафедры ОМД
        {
            double R1P = 0; double R1Z = 0; double R2P = 0; double R2Z = 0;
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];

                // среднее давление в МПа
                pass.PassRollingParameters.PSRP = 1.15 * pass.PassRollingParameters.SIGSP * Nsigma(i);
                pass.PassRollingParameters.PSRZ = 1.15 * pass.PassRollingParameters.SIGSZ * Nsigma(i);

                // полное усилие в кН
                pass.PassRollingParameters.P1P = pass.PassRollingParameters.PSRP * Kontakt(i) / 1000; // на одну нитку
                pass.PassRollingParameters.P1Z = pass.PassRollingParameters.PSRZ * Kontakt(i) / 1000; // на одну нитку

                // с учетом числа ниток Z
                pass.PassRollingParameters.P1P *= pass.Pass.Z;
                pass.PassRollingParameters.P1Z *= pass.Pass.Z;

                // определение реакции усилия в кН
                // Предполагаем, что SUMX, AKL, PDOP - это свойства в RollingStand
                R1P = pass.PassRollingParameters.P1P * pass.Pass.SUMX / pass.RollingStand.AKL;
                R2P = pass.PassRollingParameters.P1P - R1P;
                pass.PassRollingParameters.RP = Math.Max(R1P, R2P);

                R1Z = pass.PassRollingParameters.P1Z * pass.Pass.SUMX / pass.RollingStand.AKL;
                R2Z = pass.PassRollingParameters.P1Z - R1Z;
                pass.PassRollingParameters.RZ = Math.Max(R1Z, R2Z);

                pass.PassRollingParameters.KPP = pass.PassRollingParameters.RP / pass.RollingStand.PDOP;
                pass.PassRollingParameters.KPZ = pass.PassRollingParameters.RZ / pass.RollingStand.PDOP;

                // определение момента трения в шейках кН*м
                pass.PassRollingParameters.MTRP = pass.PassRollingParameters.P1P * pass.RollingStand.FPOD * pass.RollingStand.DSH / 1000;
                pass.PassRollingParameters.MTRZ = pass.PassRollingParameters.P1Z * pass.RollingStand.FPOD * pass.RollingStand.DSH / 1000;

                // крутящий момент деформации кН*м с учетом числа ниток
                pass.PassRollingParameters.MDP = 0.287 * pass.PassRollingParameters.SIGSP * pass.Pass.H1 * pass.Pass.H1 * pass.Pass.H1 * pass.PassRollingParameters.A * pass.PassRollingParameters.A * Nvalkov(i) * pass.Pass.Z / 1000 / 1000;
                pass.PassRollingParameters.MDZ = 0.287 * pass.PassRollingParameters.SIGSZ * pass.Pass.H1 * pass.Pass.H1 * pass.Pass.H1 * pass.PassRollingParameters.A * pass.PassRollingParameters.A * Nvalkov(i) * pass.Pass.Z / 1000 / 1000;

                // проверка плеча усилия прокатки
                pass.PassRollingParameters.PLECHO = (pass.PassRollingParameters.MDP + pass.PassRollingParameters.MDZ) / (2 * (pass.PassRollingParameters.P1P + pass.PassRollingParameters.P1Z) * pass.PassRollingParameters.LOD / 1000);
                if (pass.PassRollingParameters.PLECHO < 0.35)
                {
                    pass.PassRollingParameters.MDP = 2 * pass.PassRollingParameters.P1P * pass.PassRollingParameters.LOD * 0.35 / 1000;
                    pass.PassRollingParameters.MDZ = 2 * pass.PassRollingParameters.P1Z * pass.PassRollingParameters.LOD * 0.35 / 1000;
                }
                if (pass.PassRollingParameters.PLECHO > 0.75)
                {
                    pass.PassRollingParameters.MDP = 2 * pass.PassRollingParameters.P1P * pass.PassRollingParameters.LOD * 0.75 / 1000;
                    pass.PassRollingParameters.MDZ = 2 * pass.PassRollingParameters.P1Z * pass.PassRollingParameters.LOD * 0.75 / 1000;
                }

                // крутящий момент прокатки на валках кН*м
                pass.PassRollingParameters.MPRP = pass.PassRollingParameters.MDP + pass.PassRollingParameters.MTRP;
                pass.PassRollingParameters.MPRZ = pass.PassRollingParameters.MDZ + pass.PassRollingParameters.MTRZ;

                pass.PassRollingParameters.KMP = pass.PassRollingParameters.MPRP / pass.RollingStand.MDOP;
                pass.PassRollingParameters.KMZ = pass.PassRollingParameters.MPRZ / pass.RollingStand.MDOP;

                // мощность на рабочих валках кВт
                pass.PassRollingParameters.NPRP = pass.PassRollingParameters.MPRP * pass.PassRollingParameters.NR / 9.549;
                pass.PassRollingParameters.NPRZ = pass.PassRollingParameters.MPRZ * pass.PassRollingParameters.NR / 9.549;

                // затраты энергии кДж/т
                pass.PassRollingParameters.WWP = 0.131 * 1000000 * (pass.PassRollingParameters.MPRP / 9.807) * pass.PassRollingParameters.NR * pass.PassRollingParameters.TM / (pass.Pass.W * pass.PassRollingParameters.LP);
                pass.PassRollingParameters.WWZ = 0.131 * 1000000 * (pass.PassRollingParameters.MPRZ / 9.807) * pass.PassRollingParameters.NR * pass.PassRollingParameters.TM / (pass.Pass.W * pass.PassRollingParameters.LP);

                // тоже в кВт.ч./т
                pass.PassRollingParameters.WWP = pass.PassRollingParameters.WWP / 3600;
                pass.PassRollingParameters.WWZ = pass.PassRollingParameters.WWZ / 3600;

                // крутящий момент приведенный к двигателю кН*м
                pass.PassRollingParameters.MIP = pass.PassRollingParameters.MPRP / (pass.RollingStand.IR * pass.RollingStand.ETA);
                pass.PassRollingParameters.MIZ = pass.PassRollingParameters.MPRZ / (pass.RollingStand.IR * pass.RollingStand.ETA);
            }

            var mip = _context.Passes.Select(p => p.PassRollingParameters.MIP).ToList();
            var misump = _context.Passes.Select(p => p.PassRollingParameters.MISUMP).ToList();
            var kdvp = _context.Passes.Select(p => p.PassRollingParameters.KDVP).ToList();

            var miz = _context.Passes.Select(p => p.PassRollingParameters.MIZ).ToList();
            var misumz = _context.Passes.Select(p => p.PassRollingParameters.MISUMZ).ToList();
            var kdvz = _context.Passes.Select(p => p.PassRollingParameters.KDVZ).ToList();

            Console.WriteLine("MIP: ");
            foreach (var a in mip)
            {
                Console.WriteLine(a);
            }

            Console.WriteLine("MISUMP: ");
            foreach (var a in misump)
            {
                Console.WriteLine(a);
            }

            Console.WriteLine("KDVP: ");
            foreach (var a in kdvp)
            {
                Console.WriteLine(a);
            }

            Console.WriteLine("MIZ: ");
            foreach (var a in miz)
            {
                Console.WriteLine(a);
            }

            Console.WriteLine("MISUMZ: ");
            foreach (var a in misumz)
            {
                Console.WriteLine(a);
            }

            Console.WriteLine("KDVZ: ");
            foreach (var a in kdvz)
            {
                Console.WriteLine(a);
            }

            Dvigat(mip, misump, kdvp);
            Dvigat(miz, misumz, kdvz);
        }

        void Dvigat(List<double> MI, List<double> MISUM, List<double> KDVP)
        {
            int j = 1;
            int p; // j-номер двигателя p-признак привода в общем случае j не совпадает с i
            _context.Passes[_context.GlobalRollingParameters.NPR].RollingStand.PP = 1;
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var passi = _context.Passes[i];
                var passj = _context.Passes[j];
                p = (int)(passi.RollingStand.PP + 0.05);
                if (p == 1) // индивидуальный привод
                {
                    MISUM[j] = MI[i];
                    passj.PassRollingParameters.NDVR = passi.PassRollingParameters.NR * passi.RollingStand.IR; // частота вращения двигателя
                    if (passj.PassRollingParameters.NDVR <= passi.RollingStand.NDVN)
                        passj.PassRollingParameters.MDV = 9.549 * passi.RollingStand.NNOM / passi.RollingStand.NDVN; // номинальный момент двигателя
                    else
                        passj.PassRollingParameters.MDV = 9.549 * passi.RollingStand.NNOM / passj.PassRollingParameters.NDVR; // момент развиваемый двигателем
                    KDVP[j] = MISUM[j] / passj.PassRollingParameters.MDV;
                    passj.PassRollingParameters.JJ = j;
                    j++;
                }
                else
                {
                    if (p > 1) // первая клеть группового привода
                    {
                        MISUM[j] = MI[i]; // первое значение в группе
                    }
                    else
                    {
                        MISUM[j] += MI[i]; // накопление суммы момента
                    }

                    if (((int)_context.Passes[i + 1].RollingStand.PP) > 0) // последняя клеть группы
                    {
                        passj.PassRollingParameters.NDVR = passi.PassRollingParameters.NR * passi.RollingStand.IR; // частота вращения двигателя
                        if (passj.PassRollingParameters.NDVR <= passi.RollingStand.NDVN)
                            passj.PassRollingParameters.MDV = 9.549 * passi.RollingStand.NNOM / passi.RollingStand.NDVN; // номинальный момент двигателя
                        else
                            passj.PassRollingParameters.MDV = 9.549 * passi.RollingStand.NNOM / passj.PassRollingParameters.NDVR; // момент развиваемый двигателем
                        KDVP[j] = MISUM[j] / passj.PassRollingParameters.MDV;
                        passj.PassRollingParameters.JJ = j;
                        j++;
                    }
                }
            }

            // Для отладки
            //for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            //{
            //    var pass = _context.Passes[i];
            //    Console.WriteLine("Номер: " + pass.RollingStand.Number + "; NDVR: " + pass.PassRollingParameters.NDVR.ToString() + "; MDV: " + pass.PassRollingParameters.MDV + "; JJ: " + pass.PassRollingParameters.JJ);
            //}
        }

        public void Ushirenie() // расчет коэфф.уширения и ширины раската с учетом марки стали
        {
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                switch (pass.Pass.SchemaCaliber)
                {
                    case 0:
                    case 1: // гладкая бочка
                        if (pass.Pass.SchemaInput == 2) // круг
                            pass.PassRollingParameters.BETAR = Spred(0.179f, 1.357f, 0.291f, 0, 0, 0, 0.511f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 1) // прямоугольник
                            pass.PassRollingParameters.BETAR = Spred(0.0714f, 0.862f, 0.555f, 0.763f, 0, 0, 0.455f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 2: // круглый калибр
                        if (pass.Pass.SchemaInput == 4) // овал
                            pass.PassRollingParameters.BETAR = Spred(0.386f, 1.163f, 0.402f, -2.171f, 0, -1.324f, 0.616f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 5) // плоский овал
                            pass.PassRollingParameters.BETAR = Spred(0.693f, 1.286f, 0.368f, -1.052f, 0, -2.231f, 0.629f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 3: // квадратный калибр
                        if (pass.Pass.SchemaInput == 4) // овал
                            pass.PassRollingParameters.BETAR = Spred(2.242f, 1.151f, 0.352f, -2.234f, 0, -1.647f, 1.137f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 6) // шестиугольник
                            pass.PassRollingParameters.BETAR = Spred(0.360f, 0.658f, 0.202f, -0.467f, 0, -3.316f, 0.494f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        // BETAR[i]=spred(1.322,1.203,0.368,-0.852,0,-7.43,0.629,0,KOBJ[i],A[i],A0[i],AK[i],DEL0[i],PSITR[i],TANFI[i]);
                        if (pass.Pass.SchemaInput == 7) // ромб
                            pass.PassRollingParameters.BETAR = Spred(0.972f, 2.01f, 0.665f, -2.458f, 0, -1.3f, 0.7f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 4: // овальный калибр
                        if ((pass.Pass.SchemaInput == 1) || (pass.Pass.SchemaInput == 8)) // квадрат или ящичный квадрат
                            pass.PassRollingParameters.BETAR = Spred(0.377f, 0.507f, 0.316f, 0, -0.405f, 0, 1.136f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 2) // круг
                            pass.PassRollingParameters.BETAR = Spred(0.227f, 1.563f, 0.591f, 0, -0.852f, 0, 0.587f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 4) // овал
                            pass.PassRollingParameters.BETAR = Spred(0.405f, 1.163f, 0.403f, -2.171f, -0.789f, -1.324f, 0.616f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 9) // ребровой овал
                            pass.PassRollingParameters.BETAR = Spred(1.623f, 2.272f, 0.761f, -0.582f, -3.064f, 0, 0.486f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 5: // плоский овальный калибр
                        if (pass.Pass.SchemaInput == 1) // квадрат
                            pass.PassRollingParameters.BETAR = Spred(0.134f, 0.717f, 0.474f, 0, -0.507f, 0, 0.357f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 6: // шестиугольный калибр
                        if (pass.Pass.SchemaInput == 1) // квадрат
                            pass.PassRollingParameters.BETAR = Spred(2.075f, 1.848f, 0.815f, 0, -3.453f, 0, 0.659f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 7: // ромбический калибр
                        if (pass.Pass.SchemaInput == 3) // квадрат
                            pass.PassRollingParameters.BETAR = Spred(3.09f, 2.07f, 0.5f, 0, -4.85f, -4.865f, 1.543f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        if (pass.Pass.SchemaInput == 7) // ромб
                            pass.PassRollingParameters.BETAR = Spred(0.506f, 1.876f, 0.895f, -2.22f, -2.22f, -2.73f, 0.587f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 8: // ящичный калибр
                        if ((pass.Pass.SchemaInput == 8) || (pass.Pass.SchemaInput == 1)) // прямоугольник из ящичного или гл.бочки
                            pass.PassRollingParameters.BETAR = Spred(0.0714f, 0.862f, 0.746f, 0.763f, 0, 0, 0.16f, 0.362f, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 9: // ребровой овальный калибр
                        if (pass.Pass.SchemaInput == 4) // овал
                            pass.PassRollingParameters.BETAR = Spred(0.575f, 1.163f, 0.402f, -2.171f, -4.265f, -1.324f, 0.616f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                    case 10: // ребровой овальный калибр
                        if (pass.Pass.SchemaInput == 6) // шестиугольник
                            pass.PassRollingParameters.BETAR = Spred(0.3f, 1.203f, 0.368f, -0.852f, 0, -3.45f, 0.629f, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                        break;
                }
                // учет марки стали
                // базовое сопротивление деформации (Ст3)
                if ((_context.GlobalRollingParameters.K11 == 1) || (_context.GlobalRollingParameters.K11 == 3)) // по Зюзину
                {
                    pass.PassRollingParameters.SIGSBP = 9.807f * Sig1(130, 0.252f, 0.143f, 0.0025f, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSBZ = 9.807f * Sig1(130, 0.252f, 0.143f, 0.0025f, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1Z);
                }
                if (_context.GlobalRollingParameters.K11 == 2) // по Тюленеву
                {
                    pass.PassRollingParameters.SIGSBP = 9.807f * Sig2(1.41f, 9.07f, 0.124f, 0.167f, -2.54f, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSBZ = 9.807f * Sig2(1.41f, 9.07f, 0.124f, 0.167f, -2.54f, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1Z);
                }
                // расчет поправочного коэфф.
                if ((pass.PassRollingParameters.SIGSP / pass.PassRollingParameters.SIGSBP) > 1.001)
                    pass.PassRollingParameters.KBETAP = 1 + 0.6f * (double)Math.Exp(0.544f * Math.Log((pass.PassRollingParameters.SIGSP / pass.PassRollingParameters.SIGSBP) - 1));
                else
                    pass.PassRollingParameters.KBETAP = 1;
                if ((pass.PassRollingParameters.SIGSZ / pass.PassRollingParameters.SIGSBZ) > 1.001)
                    pass.PassRollingParameters.KBETAZ = 1 + 0.6f * (double)Math.Exp(0.544f * Math.Log((pass.PassRollingParameters.SIGSZ / pass.PassRollingParameters.SIGSBZ) - 1));
                else
                    pass.PassRollingParameters.KBETAZ = 1;
                // коэфф.уширения с учетом марки стали
                pass.PassRollingParameters.BETASTP = 1 + (pass.PassRollingParameters.BETAR - 1) * pass.PassRollingParameters.KBETAP;
                pass.PassRollingParameters.BETASTZ = 1 + (pass.PassRollingParameters.BETAR - 1) * pass.PassRollingParameters.KBETAZ;
                // ширина полосы после прокатки
                pass.PassRollingParameters.B1RP = pass.Pass.B0 * pass.PassRollingParameters.BETASTP;
                pass.PassRollingParameters.B1RZ = pass.Pass.B0 * pass.PassRollingParameters.BETASTZ;
                // степень заполнеия калибра металлом относительно вреза
                pass.PassRollingParameters.DELOP = pass.Pass.B1 / pass.Pass.BVR;
                pass.PassRollingParameters.DELRP = pass.PassRollingParameters.B1RP / pass.Pass.BVR;
                pass.PassRollingParameters.DELRZ = pass.PassRollingParameters.B1RZ / pass.Pass.BVR;
            }
        }

        public double Angle(double C0, double C1, double C2, double C3, double C4, double C5, double C6, double V1, double MU,
                    double M, double T1P, double PAR) // вычисление допустимого угла захвата
        {
            return C6 / (C0 + C1 * V1 * V1 - C2 * MU + C3 * M + C4 * (T1P / 1000) + C5 * PAR);
        }

        public void Ugoln() // допустимые углы захвата для непрерывных станов
        {
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                if ((pass.Pass.Schema == 18) || (pass.Pass.Schema == 88)) // прямоугольник - ящичный калибр
                    pass.PassRollingParameters.ALPHA = Angle(5.99, 0.266, 1.16, 0.42, 0.39, -1.9, 1.2, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.AZ);
                if (pass.Pass.Schema == 14) // квадрат - овал
                    pass.PassRollingParameters.ALPHA = Angle(19.1, 0.00432, 1.03, 2.67, -13.7, -0.128, 1.3, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.Pass.ROV / pass.Pass.H1);
                if (pass.Pass.Schema == 43) // овал - квадрат
                    pass.PassRollingParameters.ALPHA = Angle(16, 0.00303, 0.377, 2.7, -6.76, -7.65, 1.25, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if (pass.Pass.Schema == 16) // квадрат - шестиугольник
                    pass.PassRollingParameters.ALPHA = Angle(10.3, 0.00375, 0.653, 0.071, -2.22, -1.78, 1.23, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.AZ);
                if ((pass.Pass.Schema == 63) || (pass.Pass.Schema == 610)) // шестиугольник - квадрат или шестигранник
                    pass.PassRollingParameters.ALPHA = Angle(13.7, 0.0092, 0.77, 0.23, -3.56, -5.1, 1.17, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if ((pass.Pass.Schema == 73) || (pass.Pass.Schema == 37) || (pass.Pass.Schema == 77)) // ромбический калибр
                    pass.PassRollingParameters.ALPHA = Angle(8.82, 0.027, 0.406, 0.565, -3.23, -1.65, 1.25, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if ((pass.Pass.Schema == 42) || (pass.Pass.Schema == 44)) // овал - круг овал-овал
                    pass.PassRollingParameters.ALPHA = Angle(27.74, 0.0023, 0.44, 2.15, -19.8, -3.98, 1.25, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if (pass.Pass.Schema == 49) // овал - ребровой овал
                    pass.PassRollingParameters.ALPHA = Angle(5.56, 0.00328, 0.44, 0.0265, -0.759, -0.155, 1.12, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if ((pass.Pass.Schema == 94) || (pass.Pass.Schema == 24)) // ребровой овал - овал, круг - овал
                    pass.PassRollingParameters.ALPHA = Angle(23.54, 0.00265, 0.44, 0.374, -12.1, -5.22, 1.13, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if (pass.Pass.Schema == 52) // плоский овал - круг
                    pass.PassRollingParameters.ALPHA = Angle(9.23, 0.00284, 0.44, 0.644, 0.429, -6.32, 1.15, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.DEL0);
                if (pass.Pass.Schema == 15) // квадрат -плоский овал
                    pass.PassRollingParameters.ALPHA = Angle(7.4, 0.0024, 1.02, 0.49, -1.1, 0.18, 1.26, pass.PassRollingParameters.V1, pass.RollingStand.MU, _context.Steel.SteelCode, pass.PassRollingParameters.T1P, pass.PassRollingParameters.AZ);
                pass.PassRollingParameters.ALPHA *= 100;
                if ((pass.Pass.Schema == 10) || (pass.Pass.Schema == 20) || (pass.Pass.Schema == 11)) // гладкая бочка
                {
                    double AA = (pass.RollingStand.MU > 0.99) ? 1 : 0.8;
                    if (pass.RollingStand.MU > 1.2)
                        AA = 1.2;
                    double BB = 1;
                    if (_context.Steel.SteelCode > 1.3)
                        BB = 0.85;
                    double CC;
                    if (pass.PassRollingParameters.V1 <= 2.05)
                        CC = 1;
                    else
                        CC = 0.4 + 0.6 * Math.Exp(-0.2 * (pass.PassRollingParameters.V1 - 2));
                    pass.PassRollingParameters.ALPHA = Math.Atan(AA * BB * CC * (1.05 - 0.0005 * pass.PassRollingParameters.T1P)) * 180 / Math.PI;
                }
            }
        }

        double Otos(double C0, double C1, double C2, double C3, double C4, double C5, double C6, double DEL0, double V1,
                    double KOBJ, double PAR1, double PAR2)//расчет допустимого отношения осей
        { return (C0 - (C1 / (DEL0 * DEL0)) - C2 * V1 + (C3 / KOBJ) + C4 * PAR1 + C5 * PAR2) * C6; }

        public void Ustoichn() // расчет допустимого отношения осей
        {
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                pass.PassRollingParameters.ADOP = 1.01;
                if ((pass.Pass.Schema == 18) || (pass.Pass.Schema == 88)) // прямоугольник - ящичный калибр
                {
                    // Закомментировано для обработки данных о первом проходе
                    if (i == 1)
                        pass.PassRollingParameters.ADOP = Otos(2, -0.022, 0.01, 0.13, -1.38, 0.21, 1.18, 0.8, pass.PassRollingParameters.V1, 1 / pass.PassRollingParameters.AZ, 0.4, pass.PassRollingParameters.TANFI);
                    else if (pass.Pass.SchemaInput == 8)
                        pass.PassRollingParameters.ADOP = Otos(2, -0.022, 0.01, 0.13, -1.38, 0.21, 1.18, _context.Passes[i - 1].PassRollingParameters.DELV, pass.PassRollingParameters.V1, 1 / pass.PassRollingParameters.AZ, _context.Passes[i - 1].PassRollingParameters.TANFI, pass.PassRollingParameters.TANFI);
                    else
                        pass.PassRollingParameters.ADOP = Otos(2, -0.022, 0.01, 0.13, -1.38, 0.21, 1.18, 0.8, pass.PassRollingParameters.V1, 1 / pass.PassRollingParameters.AZ, 0.4, pass.PassRollingParameters.TANFI);
                }
                if (pass.Pass.Schema == 43) // овал квадрат
                    pass.PassRollingParameters.ADOP = Otos(4.23, 2.071, 0.012, 4.532, 0.0228, -2.42, 1.1, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, _context.Passes[i - 1].Pass.ROV / pass.Pass.B0, pass.Pass.R / pass.PassRollingParameters.HG);
                if (pass.Pass.Schema == 63 || pass.Pass.Schema == 610) // шестиугольник квадрат
                    pass.PassRollingParameters.ADOP = Otos(6.179, 1.619, 0.0335, 0.65, -0.567, -2.993, 1.2, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, pass.Pass.B0 / pass.PassRollingParameters.BK, pass.Pass.R / pass.PassRollingParameters.HG);
                if (pass.Pass.Schema == 73) // ромб - квадрат
                    pass.PassRollingParameters.ADOP = Otos(1.3, 0.338, 0.0107, 0.496, 1.134, -0.706, 1.2, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, pass.Pass.B0 / pass.PassRollingParameters.BK, pass.Pass.R / pass.PassRollingParameters.HG);
                if (pass.Pass.Schema == 77) // ромб - ромб
                    pass.PassRollingParameters.ADOP = Otos(1.3, 0.338, 0.0107, 0.496, 1.134, -0.706, 1.1, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, pass.Pass.B0 / pass.PassRollingParameters.BK, pass.Pass.R / pass.PassRollingParameters.HG);
                if ((pass.Pass.Schema == 42) || (pass.Pass.Schema == 44)) // овал -круг овал -овал
                    pass.PassRollingParameters.ADOP = Otos(1.8, 0.618, 0.005, 0.449, 0.812, 0, 1.19, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, _context.Passes[i - 1].Pass.ROV / pass.Pass.B0, 0);
                if (pass.Pass.Schema == 49) // овал - ребровой овал
                    pass.PassRollingParameters.ADOP = Otos(3.448, 0.725, 0.00282, 0.0588, 0.108, 0, 1.15, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, _context.Passes[i - 1].Pass.ROV / pass.Pass.B0, 0);
                if (pass.Pass.Schema == 52) // плоский овал -круг
                    pass.PassRollingParameters.ADOP = Otos(2.38, 0.972, 0.00201, 1.819, 0, 0, 1.12, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.V1, pass.PassRollingParameters.KOBJ, 0, 0);
                if ((pass.Pass.Schema == 10) || (pass.Pass.Schema == 11)) // гладкая бочка
                    pass.PassRollingParameters.ADOP = 2;
            }
        }

        public double Omega(int i) // расчет площади поперечного сечения полосы при заданном калибре
        {
            double cc; double aa; double bb;
            var pass = _context.Passes[i];

            switch (pass.Pass.SchemaCaliber)
            {
                case 0:
                case 1:
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.A1;
                    if (pass.Pass.SchemaInput == 2) // круг на гладких вадках
                        pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * (1 - 0.333 * (1 - Math.Sqrt(1 - 1 / (pass.PassRollingParameters.A1 * pass.PassRollingParameters.A1))));
                    else // прямоугольник на гладких валках
                        pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 2: // круглый калибр
                    pass.PassRollingParameters.WR = 0.785 - 0.667 * (1 - pass.PassRollingParameters.DEL1) * Math.Sqrt(1 - pass.PassRollingParameters.DEL1 * pass.PassRollingParameters.DEL1);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 3: // квадратный калибр
                    cc = (pass.Pass.H1 + 0.83 * pass.Pass.R) / Math.Sqrt(2);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.DEL1 * (2 - pass.PassRollingParameters.DEL1) - 0.43 * (pass.Pass.R / cc) * (pass.Pass.R / cc);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * cc * cc;
                    break;
                case 4: // овальный калибр
                    pass.PassRollingParameters.WR = 0.6 * (2.07 - pass.PassRollingParameters.DEL1) * (pass.PassRollingParameters.A1 + 0.66 * pass.PassRollingParameters.DEL1 - 0.43);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 5: // плоский овальный калибр
                    pass.PassRollingParameters.WR = (pass.PassRollingParameters.AK - 0.215) - 0.667 * (1 - pass.PassRollingParameters.DEL1) * Math.Sqrt(1 - pass.PassRollingParameters.DEL1 * pass.PassRollingParameters.DEL1);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 6: // шестиугольный калибр
                    if (pass.Pass.SchemaInput == 6) // шестигранный калибр
                        pass.PassRollingParameters.WR = 0.83 * pass.Pass.H1 / 2 * pass.Pass.H1 / 2;
                    else
                        pass.PassRollingParameters.WR = pass.Pass.H1 * (pass.Pass.BD + (1 + (1 - pass.PassRollingParameters.DEL1) / (1 - pass.Pass.BD / pass.PassRollingParameters.BK)) * (pass.Pass.B1 - pass.Pass.BD) / 2)
                              - 0.088 * pass.Pass.R * pass.Pass.R;
                    break;
                case 7: // ромбический калибр
                    pass.PassRollingParameters.WR = 0.5 * pass.PassRollingParameters.AK * pass.PassRollingParameters.DEL1 * (2 - pass.PassRollingParameters.DEL1) - 0.43 * (pass.Pass.R / pass.Pass.H1) * (pass.Pass.R / pass.Pass.H1);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 8: // ящичный калибр
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.A1 - pass.PassRollingParameters.DELV * pass.PassRollingParameters.DELV * pass.PassRollingParameters.TANFI / 2 - 0.55 * (pass.Pass.R / pass.Pass.H1) * (pass.Pass.R / pass.Pass.H1);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 9: // ребровой овальный калибр
                    aa = 1 + 1 / (pass.PassRollingParameters.AK * pass.PassRollingParameters.AK);
                    bb = 1 + 1 / pass.PassRollingParameters.AK;
                    cc = 1 / (pass.PassRollingParameters.AK * pass.PassRollingParameters.AK) - 1;
                    pass.PassRollingParameters.WR = 0.15 * pass.PassRollingParameters.AK * pass.PassRollingParameters.AK * (aa * aa * (2.07 - pass.PassRollingParameters.DEL1) * (1.66 * pass.PassRollingParameters.DEL1 - 0.43) - 0.833 * cc * bb * bb);
                    pass.PassRollingParameters.WR = pass.PassRollingParameters.WR * pass.Pass.H1 * pass.Pass.H1;
                    break;
                case 10:
                    pass.PassRollingParameters.WR = 0.866 * (pass.Pass.H1 / 1.154) * (pass.Pass.H1 / 1.154);
                    break;
            }

            // расчетный коэфф.вытяжки
            if (i == 1)
                pass.PassRollingParameters.KVITR = _context.InitialParameters.W0 / pass.PassRollingParameters.WR;
            else
                pass.PassRollingParameters.KVITR = _context.Passes[i - 1].PassRollingParameters.WR / pass.PassRollingParameters.WR;

            return pass.PassRollingParameters.WR;
        }

        // определение температуры и сопротивления деформации
        public double Sig1(double k1, double k2, double k3, double k4, double E, double KSI, double T1)
        {
            return k1 * Math.Exp(k2 * Math.Log(E)) * Math.Exp(k3 * Math.Log(KSI)) / Math.Exp(k4 * T1);
        } // по методу Зюзина

        public double Sig2(double k5, double k6, double k7, double k8, double k9, double E, double KSI, double T1)
        {
            return k5 * k6 * Math.Exp(k7 * Math.Log(KSI)) * Math.Exp(k8 * Math.Log(E)) * Math.Exp(k9 * Math.Log(T1 / 1000));
        } // по методу Тюленёва

        public double Sig3(double k1, double k2, double k3, double k4, double k5, double E, double KSI, double T1)
        {
            return k1 * Math.Exp(k2 * Math.Log(E)) * Math.Exp(k3 * Math.Log(KSI)) * Math.Exp(k5 * E) / Math.Exp(k4 * T1);
        } // по методу каф.ОМД

        public double Temp(double PP, double TT, double WW, double DTD, double T1) // температура раската
        {
            return (1000 / (Math.Exp((1.0 / 3.0) * Math.Log((0.0254 * PP * TT / WW) + Math.Pow(1000 / (T1 + DTD + 273), 3))))) - 273;
        }

        public void Tempsig() // расчет температуры полосы и сопротивления деформации, выбор показателя трения
        {
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                if (i == 1)
                {
                    pass.PassRollingParameters.LP = _context.InitialParameters.L0 * pass.PassRollingParameters.KVIT;
                    pass.PassRollingParameters.TM = pass.PassRollingParameters.LP / pass.PassRollingParameters.V1;
                    pass.PassRollingParameters.TP = _context.InitialParameters.TAU;
                    pass.PassRollingParameters.TZ = pass.PassRollingParameters.TP + pass.PassRollingParameters.TM;
                }
                else
                {
                    var prevPass = _context.Passes[i - 1];
                    pass.PassRollingParameters.LP = prevPass.PassRollingParameters.LP * pass.PassRollingParameters.KVIT;
                    pass.PassRollingParameters.TM = pass.PassRollingParameters.LP / pass.PassRollingParameters.V1;
                    pass.PassRollingParameters.TP = prevPass.RollingStand.LKL / prevPass.PassRollingParameters.V1;
                    pass.PassRollingParameters.TZ = pass.PassRollingParameters.TP + (pass.PassRollingParameters.TM - prevPass.PassRollingParameters.TM);
                }
                if (_context.GlobalRollingParameters.K13 == 9) // задана опытная температура
                {
                    pass.PassRollingParameters.T1P = pass.Pass.TOP;
                    pass.PassRollingParameters.T1Z = pass.Pass.TOP;
                }
                else // расчет температура
                {
                    if (i == 1)
                    {
                        pass.PassRollingParameters.T1P = Temp(_context.InitialParameters.P0, pass.PassRollingParameters.TP, _context.InitialParameters.W0, 0, _context.InitialParameters.T0);
                        pass.PassRollingParameters.T1Z = Temp(_context.InitialParameters.P0, pass.PassRollingParameters.TZ, _context.InitialParameters.W0, 0, _context.InitialParameters.T0);
                        pass.PassRollingParameters.DTP = _context.InitialParameters.T0 - pass.PassRollingParameters.T1P;
                        pass.PassRollingParameters.DTZ = _context.InitialParameters.T0 - pass.PassRollingParameters.T1Z;
                        pass.PassRollingParameters.DTLP = pass.PassRollingParameters.DTP;
                        pass.PassRollingParameters.DTLZ = pass.PassRollingParameters.DTZ;
                    }
                    else
                    {
                        var prevPass = _context.Passes[i - 1];
                        prevPass.PassRollingParameters.DTDP = 0.183 * prevPass.PassRollingParameters.SIGSP * Math.Log(prevPass.PassRollingParameters.KVIT);
                        prevPass.PassRollingParameters.DTDZ = 0.183 * prevPass.PassRollingParameters.SIGSZ * Math.Log(prevPass.PassRollingParameters.KVIT);
                        pass.PassRollingParameters.T1P = Temp(prevPass.PassRollingParameters.PER, pass.PassRollingParameters.TP, prevPass.Pass.W, prevPass.PassRollingParameters.DTDP, prevPass.PassRollingParameters.T1P);
                        pass.PassRollingParameters.T1Z = Temp(prevPass.PassRollingParameters.PER, pass.PassRollingParameters.TZ, prevPass.Pass.W, prevPass.PassRollingParameters.DTDZ, prevPass.PassRollingParameters.T1Z);
                        pass.PassRollingParameters.DTP = prevPass.PassRollingParameters.T1P - pass.PassRollingParameters.T1P;
                        pass.PassRollingParameters.DTZ = prevPass.PassRollingParameters.T1Z - pass.PassRollingParameters.T1Z;
                        pass.PassRollingParameters.DTLP = pass.PassRollingParameters.DTP - prevPass.PassRollingParameters.DTDP;
                        pass.PassRollingParameters.DTLZ = pass.PassRollingParameters.DTZ - prevPass.PassRollingParameters.DTDZ;
                    }
                }
                pass.PassRollingParameters.TSR = pass.PassRollingParameters.T1P; // временно принята температура переднего конца
                                                                                 // Выбор показателя трения по средней температуре
                                                                                 // double TSR=(T1P[i]+T1Z[i])/2;
                if ((pass.Pass.Schema == 37) || (pass.Pass.Schema == 73) || (pass.Pass.Schema == 77))
                {
                    pass.PassRollingParameters.PSITR = 0.5;
                    if (pass.PassRollingParameters.TSR < 1100)
                        pass.PassRollingParameters.PSITR = 0.6;
                    if (pass.PassRollingParameters.TSR < 1000)
                        pass.PassRollingParameters.PSITR = 0.75;
                }
                else
                {
                    pass.PassRollingParameters.PSITR = 0.6;
                    if (pass.PassRollingParameters.TSR < 1200)
                        pass.PassRollingParameters.PSITR = 0.7;
                    if (pass.PassRollingParameters.TSR < 1100)
                        pass.PassRollingParameters.PSITR = 0.8;
                    if (pass.PassRollingParameters.TSR < 1000)
                        pass.PassRollingParameters.PSITR = 0.9;
                    if ((pass.Pass.SchemaCaliber == 8) || (pass.Pass.SchemaCaliber == 1) || (pass.Pass.SchemaCaliber == 0))
                        pass.PassRollingParameters.PSITR = pass.PassRollingParameters.PSITR - 0.1;
                }
                if (pass.PassRollingParameters.TSR < 900)
                    pass.PassRollingParameters.PSITR = 1;
                // расчет сопротивления деформации
                if (_context.GlobalRollingParameters.K11 == 1) // по Зюзину
                {
                    pass.PassRollingParameters.SIGSP = Sig1(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSZ = Sig1(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1Z);
                }
                if (_context.GlobalRollingParameters.K11 == 2) // по Тюленеву
                {
                    pass.PassRollingParameters.SIGSP = Sig2(_context.Steel.K5, _context.Steel.K6, _context.Steel.K7, _context.Steel.K8, _context.Steel.K9, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSZ = Sig2(_context.Steel.K5, _context.Steel.K6, _context.Steel.K7, _context.Steel.K8, _context.Steel.K9, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1Z);
                }
                if (_context.GlobalRollingParameters.K11 == 3) // по методу кафедры ОМД
                {
                    pass.PassRollingParameters.SIGSP = Sig3(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, _context.Steel.K5, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSZ = Sig3(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, _context.Steel.K5, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.T1Z);
                }
                pass.PassRollingParameters.SIGSP = pass.PassRollingParameters.SIGSP * 9.807; // перевод в систему Си, МПа
                pass.PassRollingParameters.SIGSZ = pass.PassRollingParameters.SIGSZ * 9.807;
            }
        }

        public void Ochag(int i) // расчет абсолютных параметров очага деформации
        {
            var pass = _context.Passes[i];
            pass.PassRollingParameters.DELH = pass.Pass.H0 - pass.Pass.H1;
            pass.PassRollingParameters.DELB = pass.Pass.B1 - pass.Pass.B0;
            pass.PassRollingParameters.LOD = Math.Sqrt(pass.PassRollingParameters.DELH * pass.PassRollingParameters.DD / 2);
            pass.PassRollingParameters.UGZAX = Math.Asin(Math.Sqrt(pass.PassRollingParameters.DELH / (2 * pass.PassRollingParameters.DD))) * 2 * 180 / Math.PI;
        }

        public int Razmer(int Schema, int i) // передается код схемы и номер прохода
        {
            var pass = _context.Passes[i];
            pass.PassRollingParameters.HVR = (pass.Pass.H1 - pass.Pass.S) / 2; // Доопределение параметров калибров
                                                                      // KOD0[i]=Schema/10; KOD1[i]=Schema-KOD0[i]*10;
            if (!((pass.Pass.SchemaCaliber == 0) || (pass.Pass.SchemaCaliber == 1)))
                pass.PassRollingParameters.HG = pass.Pass.H1;
            double GAMMA;
            pass.PassRollingParameters.DD = (pass.RollingStand.DB + pass.Pass.S - pass.Pass.H1);
            switch (pass.Pass.SchemaCaliber)
            {
                case 0: // гладкая бочка
                case 1: // гладкая бочка
                    pass.PassRollingParameters.HVR = 0;
                    pass.PassRollingParameters.BK = 0;
                    if (pass.Pass.SchemaInput == 2)
                        pass.Pass.BD = pass.Pass.B1 - pass.Pass.H1;
                    pass.PassRollingParameters.PER = 2 * (pass.Pass.H1 + pass.Pass.B1);
                    pass.PassRollingParameters.DD = pass.RollingStand.DB;
                    break;
                case 2: // круглый калибр
                    GAMMA = 8;
                    if (pass.Pass.H1 < 100)
                        GAMMA = 11.5;
                    if (pass.Pass.H1 < 55)
                        GAMMA = 15.5;
                    if (pass.Pass.H1 < 45)
                        GAMMA = 22;
                    if (pass.Pass.H1 < 30)
                        GAMMA = 26;
                    pass.PassRollingParameters.BK = pass.Pass.H1 / Math.Cos(GAMMA / 57.3);
                    pass.PassRollingParameters.PER = Math.PI * pass.Pass.H1;
                    break;
                case 3: // квадратный калибр
                    pass.PassRollingParameters.HG = pass.Pass.H1 + 0.83 * pass.Pass.R;
                    if (pass.Pass.R > 0.001)
                        pass.PassRollingParameters.BK = pass.PassRollingParameters.HG;
                    else
                        pass.PassRollingParameters.BK = pass.Pass.BVR + pass.Pass.S;
                    pass.PassRollingParameters.PER = 2.828 * pass.PassRollingParameters.BK;
                    break;
                case 4: // овальный калибр
                    //if (pass.Pass.ROV == 0)
                    //{
                    //    throw new Exception("Не указан радиус овального калибра!");
                    //}
                    pass.PassRollingParameters.BK = pass.Pass.H1 * (Math.Sqrt((4 * pass.Pass.ROV / pass.Pass.H1) - 1));
                    pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.B1 * pass.Pass.B1 + 4 * (pass.Pass.H1 * pass.Pass.H1) / 3);
                    break;
                case 5: // плоский овальный калибр
                    pass.PassRollingParameters.BK = pass.Pass.BD + pass.Pass.H1;
                    if (pass.PassRollingParameters.BK < pass.Pass.BVR)
                        pass.PassRollingParameters.BK = pass.Pass.BVR;
                    pass.PassRollingParameters.PER = Math.PI * pass.Pass.H1 + 2 * (pass.Pass.B1 - pass.Pass.H1);
                    break;
                case 6: // шестиугольный
                case 8: // ящичный
                    if (pass.Pass.SchemaInput == 6) // шестигранный калибр
                    {
                        pass.PassRollingParameters.PER = 3 * pass.Pass.H1;
                        pass.PassRollingParameters.BK = pass.Pass.H1 * (0.866 + 0.5 * (pass.Pass.BVR - 0.866 * pass.Pass.H1) / (0.5 * pass.Pass.H1 - pass.Pass.S));
                    }
                    else
                    {
                        pass.PassRollingParameters.BK = pass.Pass.BD + pass.Pass.H1 * (pass.Pass.BVR - pass.Pass.BD) / (pass.Pass.H1 - pass.Pass.S);
                        pass.PassRollingParameters.PER = 2 * (pass.Pass.BD + pass.Pass.H1) / Math.Cos(Math.Atan((pass.Pass.BVR - pass.Pass.BD) / (pass.Pass.H1 - pass.Pass.S)));
                    }
                    break;
                case 7: // ромбический
                    pass.PassRollingParameters.HG = pass.Pass.H1 + 2 * pass.Pass.R * (Math.Sqrt(1 + 1 / (Math.Tan(118 / (2 * 57.3)) * Math.Tan(118 / (2 * 57.3)))) - 1);
                    pass.PassRollingParameters.BK = pass.Pass.BVR + (pass.Pass.BVR / (pass.PassRollingParameters.HG - pass.Pass.S)) * pass.Pass.S;
                    pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.H1 * pass.Pass.H1 + pass.Pass.B1 * pass.Pass.B1);
                    break;
                case 9: // ребровой овальный
                    pass.PassRollingParameters.BK = pass.Pass.BVR + 2 * pass.Pass.ROV * (1 - Math.Cos(Math.Asin(pass.Pass.S / (2 * pass.Pass.ROV))));
                    pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.H1 * pass.Pass.H1 + 4 * pass.Pass.B1 * pass.Pass.B1 / 3);
                    break;
                case 10:
                    pass.PassRollingParameters.BK = pass.Pass.BVR + pass.Pass.S * ((pass.Pass.BVR - pass.Pass.H1 / 1.154) / (pass.Pass.H1 / 2 - pass.Pass.S));
                    pass.PassRollingParameters.PER = 3 * pass.Pass.H1;
                    break;
                default: // получена информация о том, что такой схемы прокатки не бывает
                         // расчет прекращаем
                    return -1;
            }
            return 0;
        }
        public double Stepdef(int i, int Schema) // определение относительного обжатия
        {
            var pass = _context.Passes[i];
            pass.PassRollingParameters.E = (pass.Pass.H0 - pass.Pass.H1) / pass.Pass.H0;
            if ((Schema == 37) || (Schema == 73) || (Schema == 77) || (Schema == 24) || (Schema == 42)
               || (Schema == 44) || (Schema == 94) || (Schema == 49) || (Schema == 20)) // ********
                pass.PassRollingParameters.E = 2 * pass.PassRollingParameters.E / 3;
            if (Schema == 14)
                pass.PassRollingParameters.E = 1 - pass.Pass.H1 * (1.16 - 0.2 * pass.PassRollingParameters.DEL1) / pass.Pass.H0;
            if (Schema == 15)
                pass.PassRollingParameters.E = pass.PassRollingParameters.E * (1 + (0.031 * (pass.PassRollingParameters.KOBJ / (pass.PassRollingParameters.AK - 1) - 0.3) * (pass.PassRollingParameters.KOBJ / (pass.PassRollingParameters.AK - 1) - 0.3) - 0.01) *
                 (pass.PassRollingParameters.KOBJ * pass.PassRollingParameters.KOBJ) - 0.027 * (pass.PassRollingParameters.KOBJ / (pass.PassRollingParameters.AK - 1) - 0.5) * (pass.PassRollingParameters.KOBJ / (pass.PassRollingParameters.AK - 1) - 0.5));
            if (Schema == 16)
                pass.PassRollingParameters.E = 1 - (pass.Pass.H1 / pass.Pass.H0) + 0.5 * (1 - (pass.PassRollingParameters.AK - 1) / pass.PassRollingParameters.KOBJ) * (1 - (pass.PassRollingParameters.AK - 1) / pass.PassRollingParameters.KOBJ);
            return pass.PassRollingParameters.E;
        }

        public int Parameter(int Schema, int i) // расчет относительных параметров
        {
            var pass = _context.Passes[i];
            pass.PassRollingParameters.A1 = pass.Pass.B1 / pass.Pass.H1;
            if (pass.PassRollingParameters.BK > 0)
                pass.PassRollingParameters.AK = pass.PassRollingParameters.BK / pass.PassRollingParameters.HG;
            else
                pass.PassRollingParameters.AK = 1;
            pass.PassRollingParameters.A0 = pass.Pass.H0 / pass.Pass.B0;
            if (pass.PassRollingParameters.BK == 0) 
                pass.PassRollingParameters.DEL1 = 1;
            else
                pass.PassRollingParameters.DEL1 = pass.Pass.B1 / pass.PassRollingParameters.BK;
            pass.PassRollingParameters.KOBJ = pass.Pass.H0 / pass.Pass.H1;
            pass.PassRollingParameters.KUSH = pass.Pass.B1 / pass.Pass.B0;
            pass.PassRollingParameters.A = pass.PassRollingParameters.DD / pass.Pass.H1;
            if (pass.PassRollingParameters.DEL1 > 1) // переполнение калибра и продолжение расчета
            {
                pass.PassRollingParameters.DELOP = pass.PassRollingParameters.DEL1;
                pass.PassRollingParameters.DEL1 = 1;
            }
            if ((Schema == 16) || (Schema == 18) || (Schema == 88) || (Schema == 15))
                pass.PassRollingParameters.AZ = pass.Pass.B0 / pass.Pass.BD;
            else
                pass.PassRollingParameters.AZ = 0;
            if ((Schema == 18) || (Schema == 88) || (Schema == 16))
            {
                pass.PassRollingParameters.TANFI = (pass.PassRollingParameters.BK - pass.Pass.BD) / pass.Pass.H1;
                pass.PassRollingParameters.DELV = (pass.Pass.B1 - pass.Pass.BD) / (pass.PassRollingParameters.BK - pass.Pass.BD);
            }
            else
            {
                if (Schema == 610)
                    pass.PassRollingParameters.TANFI = (pass.PassRollingParameters.BK - 0.866 * pass.Pass.H1) / (0.5 * pass.Pass.H1);
                else
                    pass.PassRollingParameters.TANFI = 0;
                pass.PassRollingParameters.DELV = 0;
            }
            if (Math.Abs(pass.Pass.W) < 0.1) // отсутствует значение площади поперечного сечения
                pass.Pass.W = Omega(i);
            if (i == 1)
            {
                pass.PassRollingParameters.KVIT = _context.InitialParameters.W0 / pass.Pass.W;
                // Исправление первого прохода
                // DEL0[i]=1;

            }
            else
            {
                var prevPass = _context.Passes[i - 1];
                pass.PassRollingParameters.KVIT = prevPass.Pass.W / pass.Pass.W;
                pass.PassRollingParameters.DEL0 = prevPass.PassRollingParameters.DEL1;
            }
            if ((pass.PassRollingParameters.KVIT < 1.0001) || (pass.PassRollingParameters.KOBJ < 1.0001)) // дать информацию о том что коэфф.вытяжки или обжатия <1
                return -1; // расчет прекращаем
                           // определение относительного обжатия
            Stepdef(i, Schema);
            return 0;
        }

        public void Skorost2() // вычисление скоростного режима полунепрерывных станов
        {
            // с петлевой группой
            int i = 1;
            while (i <= _context.GlobalRollingParameters.NPR)
            {
                var pass = _context.Passes[i];
                pass.PassRollingParameters.NN = pass.RollingStand.NDVN / pass.RollingStand.IR; // частота валков номинальная
                pass.PassRollingParameters.NMIN = pass.RollingStand.NDVMIN / pass.RollingStand.IR; // частота валков минимальная
                pass.PassRollingParameters.NMAX = pass.RollingStand.NDVMAX / pass.RollingStand.IR; // частота валков максимальная
                                                                                          // расчет скоростного режима
                pass.PassRollingParameters.DK = (pass.RollingStand.DB + pass.Pass.S) - pass.Pass.W / pass.Pass.B1;
                // i = i++; // Эта строка в C++ не имеет эффекта и удалена
                i++; // Исправлен инкремент
            }
            i = _context.GlobalRollingParameters.NPR;
            while (i >= 1)
            {
                var pass = _context.Passes[i];
                if (i == _context.GlobalRollingParameters.NPR)
                {
                    pass.PassRollingParameters.V1 = _context.InitialParameters.VK;
                    pass.PassRollingParameters.NR = 60000 * pass.PassRollingParameters.V1 / (Math.PI * pass.PassRollingParameters.DK);
                }
                else
                {
                    var nextPass = _context.Passes[i + 1];
                    if ((nextPass.RollingStand.PP < 0.9999) && (pass.RollingStand.PP > 0.1)) // наличие петлевой группы
                    {
                        pass.PassRollingParameters.NR = nextPass.PassRollingParameters.NR;
                        pass.PassRollingParameters.V1 = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NR / 60000;
                    }
                    else
                    {
                        pass.PassRollingParameters.V1 = nextPass.PassRollingParameters.V1 / nextPass.PassRollingParameters.KVIT;
                        pass.PassRollingParameters.NR = 60000 * pass.PassRollingParameters.V1 / (Math.PI * pass.PassRollingParameters.DK);
                        if (nextPass.RollingStand.PP < 0.5) // спаренный привод
                            pass.PassRollingParameters.NR = nextPass.PassRollingParameters.NR * nextPass.RollingStand.IR / pass.RollingStand.IR;
                    }
                }
                if (_context.GlobalRollingParameters.K12 == 1) // заданы фактические обороты
                {
                    pass.PassRollingParameters.NR = pass.RollingStand.NZ;
                    pass.PassRollingParameters.V1 = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NR / 60000;
                }
                pass.PassRollingParameters.VMAX = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NMAX * (1 - _context.InitialParameters.LR) / 60000;
                pass.PassRollingParameters.VMIN = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NMIN * (1 + _context.InitialParameters.LR) / 60000;
                pass.PassRollingParameters.KSI = 0.105 * pass.PassRollingParameters.NR * Math.Sqrt(pass.PassRollingParameters.E * pass.PassRollingParameters.DK / (2 * pass.Pass.H0));
                i--;
            }
        }

        public void Skorost(int TS, int K12) // вычисление скоростного режима
        {
            int i = 1; // для непрерывных и последовательных станов
            while (i <= _context.GlobalRollingParameters.NPR)
            {
                var pass = _context.Passes[i];
                pass.PassRollingParameters.NN = pass.RollingStand.NDVN / pass.RollingStand.IR; // частота валков номинальная
                pass.PassRollingParameters.NMIN = pass.RollingStand.NDVMIN / pass.RollingStand.IR; // частота валков минимальная
                pass.PassRollingParameters.NMAX = pass.RollingStand.NDVMAX / pass.RollingStand.IR; // частота валков максимальная
                                                                                          // расчет скоростного режима
                if (TS == 1) // стан непрерывный
                {
                    double V0 = _context.InitialParameters.VK / (_context.InitialParameters.W0 / _context.Passes[_context.GlobalRollingParameters.NPR].Pass.W); // СКОРОСТЬ ВХОДА В СТАН
                    if (i == 1) // 1 проход
                        pass.PassRollingParameters.V1 = V0 * pass.PassRollingParameters.KVIT; // скорость в 1 проходе
                    else
                    {
                        var prevPass = _context.Passes[i - 1];
                        pass.PassRollingParameters.V1 = prevPass.PassRollingParameters.V1 * pass.PassRollingParameters.KVIT; // скорость в остальных проходах
                    }
                    pass.PassRollingParameters.DK = (pass.RollingStand.DB + pass.Pass.S) - pass.Pass.W / pass.Pass.B1; // катающий диаметр
                    pass.PassRollingParameters.NR = pass.PassRollingParameters.V1 / (Math.PI * pass.PassRollingParameters.DK) * 60000; // частота вращения
                    if (pass.RollingStand.PP < 0.5) // спаренный привод
                    {
                        var prevPass = _context.Passes[i - 1];
                        pass.PassRollingParameters.NR = prevPass.PassRollingParameters.NR * prevPass.RollingStand.IR / pass.RollingStand.IR;
                    }
                    if (K12 == 1) // заданы фактические обороты
                    {
                        pass.PassRollingParameters.NR = pass.RollingStand.NZ;
                        pass.PassRollingParameters.V1 = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NR / 60000;
                    }
                }
                else if (TS == 2) // стан последовательный
                {
                    pass.PassRollingParameters.NR = pass.RollingStand.NZ; // частота вращения всегда через заданную
                    pass.PassRollingParameters.DK = (pass.RollingStand.DB + pass.Pass.S) - pass.Pass.W / pass.Pass.B1; // катающий диаметр
                    pass.PassRollingParameters.V1 = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NR / 60000; // скорость в любом проходе
                }
                pass.PassRollingParameters.VMAX = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NMAX * (1 - _context.InitialParameters.LR) / 60000;
                pass.PassRollingParameters.VMIN = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NMIN * (1 + _context.InitialParameters.LR) / 60000;
                pass.PassRollingParameters.KSI = 0.105 * pass.PassRollingParameters.NR * Math.Sqrt(pass.PassRollingParameters.E * pass.PassRollingParameters.DK / (2 * pass.Pass.H0));
                i++;
            }
        }


        public void Raskat(int i) // расчет размеров полос, площадей, поперечных сечений и др. параметров
        {
            var pass = _context.Passes[i];
            Razmer(pass.Pass.Schema, i); // доопределение параметров калибров
            if (_context.GlobalRollingParameters.K17 > 0) // размеры полос неизвестны
                Forma(i, pass.Pass.Schema); // расчет размеров полос по проходам
            Parameter(pass.Pass.Schema, i); // относительные параметры прокатки
            Ochag(i); // абсолютные параметры очага деформации 1
            if (_context.GlobalRollingParameters.K14 == 9) // расчет площади поперечного сечения полосы
                pass.PassRollingParameters.WR = Omega(i);
        }

        public double Kontakt(int i) // расчет контактной поверхности
        {
            var pass = _context.Passes[i];
            pass.PassRollingParameters.FKON = 0.5 * pass.Pass.H1 * (pass.Pass.B1 + pass.Pass.B0) * Math.Sqrt(0.5 * pass.PassRollingParameters.A * (pass.PassRollingParameters.KOBJ - 1));

            if (pass.Pass.Schema == 14) // квадрат - овал
            {
                double aa = pass.PassRollingParameters.KOBJ - 1;
                double bb = 0.71 * pass.PassRollingParameters.DEL1 + 0.29;
                double cc = 0.375 * pass.PassRollingParameters.AK + 0.845;
                double ee = bb * (0.28 * aa * aa + cc * aa + 0.09 * pass.PassRollingParameters.AK + 0.213);
                pass.PassRollingParameters.FKON = pass.Pass.H1 * pass.Pass.H1 * ee * Math.Sqrt((pass.PassRollingParameters.A + 1) / pass.PassRollingParameters.KOBJ - 0.75);
            }

            if (pass.Pass.Schema == 43) // овал - квадрат
            {
                var prevPass = _context.Passes[i - 1];
                double aa = (prevPass.PassRollingParameters.AK - 0.2) * (prevPass.PassRollingParameters.AK - 0.2);
                double bb = Math.Sqrt((pass.PassRollingParameters.KOBJ / pass.PassRollingParameters.DEL0) + 0.4);
                double cc = (pass.PassRollingParameters.DEL0 - 0.1) * (pass.PassRollingParameters.DEL0 - 0.1);
                double ee = (0.23 + (1.86 + 6.7 / aa) * (bb - 1.2)) * (0.41 - 0.037 / cc) * (0.8 * pass.PassRollingParameters.DEL1 + 0.36);
                pass.PassRollingParameters.FKON = pass.Pass.H1 * pass.Pass.H1 * ee * Math.Sqrt(pass.PassRollingParameters.A);
            }
            if ((pass.Pass.Schema == 77) || (pass.Pass.Schema == 37) || (pass.Pass.Schema == 73)) // ромбические калибры
            {
                var prevPass = _context.Passes[i - 1];
                double aa = (prevPass.PassRollingParameters.AK * pass.PassRollingParameters.AK - 1);
                double bb = pass.PassRollingParameters.KOBJ - 1;
                double cc = pass.PassRollingParameters.KOBJ / pass.PassRollingParameters.DEL0;
                double ee = (0.707 * pass.PassRollingParameters.AK * ((0.6 * cc + 0.4 - pass.PassRollingParameters.KOBJ) * Math.Sqrt(bb) + 2 * (Math.Sqrt(bb * bb * bb)) / 3) +
                          0.283 * pass.PassRollingParameters.AK * (pass.PassRollingParameters.DEL1 * pass.PassRollingParameters.AK * pass.PassRollingParameters.AK * prevPass.PassRollingParameters.AK - 1) * Math.Sqrt(bb)) / aa;
                pass.PassRollingParameters.FKON = pass.Pass.H1 * pass.Pass.H1 * ee * Math.Sqrt(pass.PassRollingParameters.A - 0.25);
            }
            if ((pass.Pass.Schema == 44) || (pass.Pass.Schema == 42) || (pass.Pass.Schema == 24) || (pass.Pass.Schema == 49) ||
                (pass.Pass.Schema == 94)) // овальные, круглые и ребровые овальные
            {
                double aa = pass.PassRollingParameters.A1 / Math.Sqrt(2);
                double bb = 1.62 - pass.PassRollingParameters.DEL0;
                double ee;
                if ((pass.Pass.Schema == 94) || (pass.Pass.Schema == 24))
                    ee = aa * bb * (1 + 0.4 / pass.PassRollingParameters.A1);
                else
                    ee = aa * bb * (1 - 0.1 * pass.PassRollingParameters.AK);
                pass.PassRollingParameters.FKON = pass.Pass.H1 * pass.Pass.H1 * ee * Math.Sqrt(pass.PassRollingParameters.A * (pass.PassRollingParameters.KOBJ - 1));
            }

            if (pass.Pass.Schema == 20) // КРУГ - ГЛАДКАЯ БОЧКА
                pass.PassRollingParameters.FKON = pass.PassRollingParameters.FKON * 0.785;

            return pass.PassRollingParameters.FKON;
        }

        public double Nsigma(int i) // расчет коэфф. напряженного состояния
        {
            double aa; double bb; // фактор формы
            var pass = _context.Passes[i];
            pass.PassRollingParameters.LH = (Math.Sqrt(2 * pass.PassRollingParameters.A * (pass.PassRollingParameters.KOBJ - 1))) / (pass.PassRollingParameters.KOBJ + 1);
            switch (pass.Pass.SchemaCaliber)
            {
                case 0:
                case 1: // гладкая бочка
                        // исходный прямоугольник
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 4.23 + 18.55 / (pass.PassRollingParameters.LH + 3)) * (1.108 - 0.102 * Math.Sqrt(pass.PassRollingParameters.A0)) * (0.68 + 0.32 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 2) // исходный круг
                        pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 4.55 + 17.1 / (pass.PassRollingParameters.LH + 2)) * (0.64 + 0.36 * pass.PassRollingParameters.PSITR);
                    break;
                case 8: // ящичный калибр
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 5.55 + 37 / (pass.PassRollingParameters.LH + 5)) * (0.0488 * pass.PassRollingParameters.A + 0.534 * pass.PassRollingParameters.A / (pass.PassRollingParameters.A - 1)) * (0.745 + 0.051 / (pass.PassRollingParameters.TANFI + 0.1))
                            * (1.108 - 0.102 * Math.Sqrt(pass.PassRollingParameters.A0)) * (1.225 - 0.18 / pass.PassRollingParameters.PSITR);
                    break;
                case 9: // ребровой овал
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 3.17 + 16.6 / (pass.PassRollingParameters.LH + 3)) * (1.15 - 0.075 * pass.PassRollingParameters.A0) * (0.88 + 0.16 * pass.PassRollingParameters.PSITR);
                    break;
                case 5: // плоский овал
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 8 + 44.38 / (pass.PassRollingParameters.LH + 4)) * (1.747 - 22.27 / (pass.PassRollingParameters.A + 20)) * (1.08 - 0.18 / pass.PassRollingParameters.AK) * (1.566 - 0.737 / (pass.PassRollingParameters.PSITR + 0.5));
                    break;
                case 2: // круг
                        // овал
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 3.7 + 18 / (pass.PassRollingParameters.LH + 3)) * (1.15 - 0.075 * pass.PassRollingParameters.A0) * (0.88 + 0.16 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 5) // плоский овал
                        pass.PassRollingParameters.NSIG = (2 * pass.PassRollingParameters.LH - 10 + 30.5 / (pass.PassRollingParameters.LH + 2)) * (0.875 + 0.0694 * pass.PassRollingParameters.A0) * (1.322 - 1.8 / Math.Sqrt(pass.PassRollingParameters.A + 2)) *
                                (0.7 + 0.375 * pass.PassRollingParameters.PSITR);
                    break;
                case 4: // овал
                    aa = (pass.PassRollingParameters.AK * pass.PassRollingParameters.AK + 1) / (pass.PassRollingParameters.AK * pass.PassRollingParameters.AK);
                    bb = (pass.PassRollingParameters.LH - 6.14 + 40.5 / (pass.PassRollingParameters.LH + 5));
                    if ((pass.Pass.SchemaInput == 2) || (pass.Pass.SchemaInput == 9)) // круг ребровой овал
                        pass.PassRollingParameters.NSIG = 0.9 * bb * aa * (0.63 + 0.37 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.NSIG = 0.92 * bb * aa * (0.6 + 0.4 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 1) // квадрат
                        pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 8.9 + 83 / (pass.PassRollingParameters.LH + 8)) * (0.8 + 0.8 / pass.PassRollingParameters.AK) * (0.61 + 0.39 * pass.PassRollingParameters.PSITR);
                    break;
                case 7: // ромб
                        // ромб
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 4.96 + 16.8 / (pass.PassRollingParameters.LH + 2)) * (0.815 + 0.087 * pass.PassRollingParameters.A) * (0.1 + pass.PassRollingParameters.DEL0) * (0.885 + 0.192 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 3) // квадрат
                        pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 5.44 + 17.9 / (pass.PassRollingParameters.LH + 2)) * ((pass.PassRollingParameters.A + 2) / (0.45 * pass.PassRollingParameters.A + 4.75)) * (1.1 * pass.PassRollingParameters.DEL0 - 0.045) *
                                 (0.829 + 0.285 * pass.PassRollingParameters.PSITR);
                    break;
                case 3: // квадрат
                        // овал
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 2.28 + 10 / (pass.PassRollingParameters.LH + 2)) * (0.178 + 0.902 * pass.PassRollingParameters.DEL0) * (0.8 + 0.25 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.NSIG = ((pass.PassRollingParameters.LH + 1.86) - (pass.PassRollingParameters.LH / (pass.PassRollingParameters.LH + 1)) * (3.52 - 2.4 * pass.PassRollingParameters.DEL0 / pass.PassRollingParameters.A0)) * (0.65 + 0.35 * pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 7) // ромб
                        pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 6.92 + 39.2 / (pass.PassRollingParameters.LH + 4)) * ((pass.PassRollingParameters.A + 2) / (0.693 * pass.PassRollingParameters.A + 3.54)) * (1.05 * pass.PassRollingParameters.DEL0 + 0.055) *
                                 (0.829 + 0.285 * pass.PassRollingParameters.PSITR);
                    break;
                case 6: // шестиугольник
                        // квадрат
                    pass.PassRollingParameters.NSIG = (pass.PassRollingParameters.LH - 7.76 + 42.8 / (pass.PassRollingParameters.LH + 4)) * (0.172 + 0.185 * Math.Sqrt(pass.PassRollingParameters.A + 10)) * (1.088 - 0.105 / (pass.PassRollingParameters.AK - 1)) *
                            (2.42 - 2.56 / (pass.PassRollingParameters.PSITR + 1));
                    if (pass.Pass.SchemaInput == 6) // шестигранник
                        pass.PassRollingParameters.NSIG = ((pass.PassRollingParameters.LH + 1.86) - (pass.PassRollingParameters.LH / (pass.PassRollingParameters.LH + 1)) * (3.52 - 2.4 * pass.PassRollingParameters.DEL0 / pass.PassRollingParameters.A0)) * (0.65 + 0.35 * pass.PassRollingParameters.PSITR);
                    break;
                case 10: // Шестигранник
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.NSIG = ((pass.PassRollingParameters.LH + 1.86) - (pass.PassRollingParameters.LH / (pass.PassRollingParameters.LH + 1)) * (3.52 - 2.4 * pass.PassRollingParameters.DEL0 / pass.PassRollingParameters.A0)) * (0.65 + 0.35 * pass.PassRollingParameters.PSITR);
                    break;
            }
            return pass.PassRollingParameters.NSIG;
        }

        public double Nvalkov(int i) // расчет коэфф. мощности
        {
            double aa; double bb; double cc;
            var pass = _context.Passes[i];
            switch (pass.Pass.SchemaCaliber)
            {
                case 0: // гладкая бочка
                case 1:
                    aa = pass.PassRollingParameters.KOBJ - 1;
                    bb = 0.05 + 4.8 / pass.PassRollingParameters.A;
                    cc = (55 / pass.PassRollingParameters.A) - 1.1;
                    // исходный прямоугольник
                    pass.PassRollingParameters.NVAL = bb * (Math.Exp(0.68 * aa) - 1) * (0.68 + 0.32 * pass.PassRollingParameters.PSITR) / pass.PassRollingParameters.A0;
                    if (pass.Pass.SchemaInput == 2) // исходный круг
                        pass.PassRollingParameters.NVAL = aa * aa * ((100 / pass.PassRollingParameters.A) - 1 - aa * cc) * (0.069 + 0.039 * pass.PassRollingParameters.PSITR);
                    break;
                case 8: // ящичный калибр
                    aa = pass.PassRollingParameters.KOBJ - 1;
                    bb = 0.0105 - 0.0012 * pass.PassRollingParameters.A0 * pass.PassRollingParameters.A0;
                    cc = (pass.PassRollingParameters.A0 + 0.1) * (pass.PassRollingParameters.A0 + 0.1);
                    pass.PassRollingParameters.NVAL = pass.PassRollingParameters.KOBJ * aa * (0.25 + (bb / pass.PassRollingParameters.TANFI) - 0.024 * pass.PassRollingParameters.A0 * pass.PassRollingParameters.A0 + 0.254 * (1.02 - 0.2 * pass.PassRollingParameters.TANFI) / cc) *
                    (0.315 + 3.425 / (pass.PassRollingParameters.A - 1)) * (1.225 - 0.18 / pass.PassRollingParameters.PSITR);
                    break;
                case 4: // овал
                    aa = pass.PassRollingParameters.KVIT;
                    // квадрат
                    pass.PassRollingParameters.NVAL = aa * (aa - 1) * (9 / (pass.PassRollingParameters.A + 10)) * (1.36 - 0.36 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 2) // круг
                        pass.PassRollingParameters.NVAL = (aa * aa - 1) * (0.094 + 3.66 / (pass.PassRollingParameters.A + 5)) * (1.31 - 0.31 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.NVAL = (aa * aa - 1) * (0.1 + 2.7 / (pass.PassRollingParameters.A + 5)) * (1.36 - 0.27 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 9) // ребровой овал
                        pass.PassRollingParameters.NVAL = (aa * aa - 1) * (0.013 + 4.4 / (pass.PassRollingParameters.A + 5)) * (1.31 - 0.31 / pass.PassRollingParameters.PSITR);
                    break;
                case 2: // круг
                    aa = pass.PassRollingParameters.KVIT;
                    // овал
                    pass.PassRollingParameters.NVAL = aa * (aa - 1) * (0.115 + 2.3 / (pass.PassRollingParameters.A + 2)) * (1.36 - 0.27 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 5) // плоский овал
                        pass.PassRollingParameters.NVAL = aa * (aa - 1) * (0.078 + 2.6 / (pass.PassRollingParameters.A + 2)) * (1.36 - 0.27 / pass.PassRollingParameters.PSITR);
                    break;
                case 9: // ребровой овал
                    aa = pass.PassRollingParameters.KVIT;
                    pass.PassRollingParameters.NVAL = aa * (aa - 1) * (0.09 + 1.84 / (pass.PassRollingParameters.A + 2)) * (1.36 - 0.27 / pass.PassRollingParameters.PSITR);
                    break;
                case 5: // плоский овал
                    aa = pass.PassRollingParameters.KVIT;
                    pass.PassRollingParameters.NVAL = aa * (aa - 1) * (0.15 + 4.3 / (pass.PassRollingParameters.A + 2)) * (1.46 - 0.69 / (pass.PassRollingParameters.PSITR + 0.5));
                    break;
                case 3: // квадрат
                    aa = pass.PassRollingParameters.KVIT;
                    // овал
                    pass.PassRollingParameters.NVAL = aa * aa * (aa - 1) * (0.01 + 2.3 / (pass.PassRollingParameters.A + 5)) * (1.2 - 0.16 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.NVAL = aa * aa * (aa - 1) * (0.045 + 1.2 / (pass.PassRollingParameters.A + 2)) * (1.74 - 1.12 / (pass.PassRollingParameters.PSITR + 0.5));
                    if (pass.Pass.SchemaInput == 7) // ромб
                        pass.PassRollingParameters.NVAL = (aa - 1) * (1.98 - 0.58 / pass.PassRollingParameters.PSITR) / (0.93 - 12 * (0.9 - pass.PassRollingParameters.DEL0) * (0.9 - pass.PassRollingParameters.DEL0) +
                        ((0.648 / pass.PassRollingParameters.DEL0) - 0.56) * pass.PassRollingParameters.A);
                    break;
                case 6: // шестиугольник
                    aa = pass.PassRollingParameters.KVIT;
                    // квадрат
                    pass.PassRollingParameters.NVAL = (aa * aa - 1) * (0.152 + 1.36 / pass.PassRollingParameters.A) * (2.48 - 2.56 / (pass.PassRollingParameters.PSITR + 1));
                    if (pass.Pass.SchemaInput == 6) // шестигранник
                        pass.PassRollingParameters.NVAL = aa * aa * (aa - 1) * (0.045 + 1.2 / (pass.PassRollingParameters.A + 2)) * (1.74 - 1.12 / (pass.PassRollingParameters.PSITR + 0.5));
                    break;
                case 7: // ромб
                    aa = pass.PassRollingParameters.KVIT;
                    // ромб
                    pass.PassRollingParameters.NVAL = (aa - 1) * (1.65 - 0.39 / pass.PassRollingParameters.PSITR) / (pass.PassRollingParameters.DEL0 - 0.4 + (0.75 - 0.625 * pass.PassRollingParameters.DEL0) * pass.PassRollingParameters.A);
                    if (pass.Pass.SchemaInput == 3) // квадрат
                        pass.PassRollingParameters.NVAL = (aa - 1) * (1.6 - 0.36 / pass.PassRollingParameters.PSITR) / (1.6 * pass.PassRollingParameters.DEL0 - 1.11 + (0.674 - 0.54 * pass.PassRollingParameters.DEL0) * pass.PassRollingParameters.A);
                    break;
                case 10: // Шестигранник
                    aa = pass.PassRollingParameters.KVIT;
                    // овал
                    pass.PassRollingParameters.NVAL = aa * aa * (aa - 1) * (0.01 + 2.3 / (pass.PassRollingParameters.A + 5)) * (1.2 - 0.16 / pass.PassRollingParameters.PSITR);
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.NVAL = aa * aa * (aa - 1) * (0.045 + 1.2 / (pass.PassRollingParameters.A + 2)) * (1.74 - 1.12 / (pass.PassRollingParameters.PSITR + 0.5));
                    break;
            }
            return pass.PassRollingParameters.NVAL;
        }

        //void ResultOutput()
        //{
        //    Domain();
        //}

        public void SpPolosa() // Расчет энергосиловых параметров по методу Соответственной полосы
        {
            // пересчет с фасонных полос на соответственные
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                int metod = (int)(pass.Pass.SP + 0.01);
                if (metod == 1) // пересчет по методу Врацкого
                {
                    pass.PassRollingParameters.B1SP = pass.Pass.B1;
                    pass.PassRollingParameters.H1SP = pass.Pass.W / pass.PassRollingParameters.B1SP;
                    pass.PassRollingParameters.B0SP = pass.Pass.B0;
                    if (i == 1) // первый проход
                        pass.PassRollingParameters.H0SP = _context.InitialParameters.W0 / pass.PassRollingParameters.B0SP;
                    else
                        pass.PassRollingParameters.H0SP = _context.Passes[i - 1].Pass.W / pass.PassRollingParameters.B0SP;
                }
                else // пересчет по методу Головина
                {
                    pass.PassRollingParameters.H1SP = Math.Sqrt(pass.Pass.W / pass.PassRollingParameters.A1);
                    pass.PassRollingParameters.B1SP = pass.PassRollingParameters.H1SP * pass.PassRollingParameters.A1;
                    if (i == 1) // первый проход
                        pass.PassRollingParameters.H0SP = Math.Sqrt(_context.InitialParameters.W0 / (pass.Pass.B0 / pass.Pass.H0));
                    else
                        pass.PassRollingParameters.H0SP = Math.Sqrt(_context.Passes[i - 1].Pass.W) / (pass.Pass.B0 / pass.Pass.H0);
                    pass.PassRollingParameters.B0SP = pass.PassRollingParameters.H0SP * (pass.Pass.B0 / pass.Pass.H0);
                }
                // средняя степень деформации
                pass.PassRollingParameters.ESP = (pass.PassRollingParameters.H0SP - pass.PassRollingParameters.H1SP) / pass.PassRollingParameters.H0SP;
                // средняя скорость деформации
                pass.PassRollingParameters.KSISP = 0.105 * pass.PassRollingParameters.NR * Math.Sqrt(pass.PassRollingParameters.ESP * (pass.RollingStand.DB + pass.Pass.S - pass.PassRollingParameters.H1SP) / (2 * pass.PassRollingParameters.H0SP));
                // расчет сопротивления деформации  МПа
                // расчет сопротивления деформации
                if (_context.GlobalRollingParameters.K11 == 1) // по Зюзину
                {
                    pass.PassRollingParameters.SIGSSPP = Sig1(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSSPZ = Sig1(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1Z);
                }
                if (_context.GlobalRollingParameters.K11 == 2) // по Тюленеву
                {
                    pass.PassRollingParameters.SIGSSPP = Sig2(_context.Steel.K5, _context.Steel.K6, _context.Steel.K7, _context.Steel.K8, _context.Steel.K9, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSSPZ = Sig2(_context.Steel.K5, _context.Steel.K6, _context.Steel.K7, _context.Steel.K8, _context.Steel.K9, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1Z);
                }
                if (_context.GlobalRollingParameters.K11 == 3) // по методу кафедры ОМД
                {
                    pass.PassRollingParameters.SIGSSPP = Sig3(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, _context.Steel.K5, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1P);
                    pass.PassRollingParameters.SIGSSPZ = Sig3(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, _context.Steel.K5, pass.PassRollingParameters.ESP, (pass.PassRollingParameters.KSISP > 120) ? 120 : pass.PassRollingParameters.KSISP, pass.PassRollingParameters.T1Z);
                }
                pass.PassRollingParameters.SIGSSPP = pass.PassRollingParameters.SIGSSPP * 9.807; // перевод в систему Си, МПа
                pass.PassRollingParameters.SIGSSPZ = pass.PassRollingParameters.SIGSSPZ * 9.807;
                // коэфф. трения по Гету при средней температуре
                pass.PassRollingParameters.MUSP = 0.55 - 0.00024 * (pass.PassRollingParameters.T1P + pass.PassRollingParameters.T1Z) / 2;
                // длина очага деформации
                pass.PassRollingParameters.LSP = Math.Sqrt((pass.RollingStand.DB + pass.Pass.S - pass.PassRollingParameters.H1SP) * (pass.PassRollingParameters.H0SP - pass.PassRollingParameters.H1SP) / 2);
                // параметр формы очага деформации
                pass.PassRollingParameters.DELSP = 2 * pass.PassRollingParameters.MUSP * pass.PassRollingParameters.LSP / (pass.PassRollingParameters.H0SP - pass.PassRollingParameters.H1SP);
                // Коэфф. напряженного состояния
                pass.PassRollingParameters.NSIGSP = 1 + pass.PassRollingParameters.DELSP * (1 - Math.Sqrt(1 - pass.PassRollingParameters.ESP)) * (1 - Math.Sqrt(1 - pass.PassRollingParameters.ESP)) / pass.PassRollingParameters.ESP;
                if (pass.PassRollingParameters.NSIGSP < 1) pass.PassRollingParameters.NSIGSP = 1;
                // средняя высота полосы
                pass.PassRollingParameters.HSRSP = (pass.PassRollingParameters.H0SP + pass.PassRollingParameters.H1SP) / 2;
                // фактор формы очага деформации
                pass.PassRollingParameters.LHSR = pass.PassRollingParameters.LSP / pass.PassRollingParameters.HSRSP;
                // коэфф. жестких концов
                if (pass.PassRollingParameters.LHSR < 1)
                    pass.PassRollingParameters.NGSP = 2 - Math.Sqrt(pass.PassRollingParameters.LHSR);
                else
                    pass.PassRollingParameters.NGSP = 1;
                // среднее давление прокатки   МПа
                pass.PassRollingParameters.PSRSPP = 1.08 * pass.PassRollingParameters.NSIGSP * pass.PassRollingParameters.NGSP * pass.PassRollingParameters.SIGSSPP;
                pass.PassRollingParameters.PSRSPZ = 1.08 * pass.PassRollingParameters.NSIGSP * pass.PassRollingParameters.NGSP * pass.PassRollingParameters.SIGSSPZ;
                // контактная поверхность мм*мм
                pass.PassRollingParameters.FKONSP = (pass.PassRollingParameters.B0SP + pass.PassRollingParameters.B1SP) * pass.PassRollingParameters.LSP / 2;
                // усилие прокатки с учетом числа ниток Z kN
                pass.PassRollingParameters.P1SPP = pass.PassRollingParameters.PSRSPP * pass.PassRollingParameters.FKONSP / 1000;
                pass.PassRollingParameters.P1SPP *= pass.Pass.Z;
                pass.PassRollingParameters.P1SPZ = pass.PassRollingParameters.PSRSPZ * pass.PassRollingParameters.FKONSP / 1000;
                pass.PassRollingParameters.P1SPZ *= pass.Pass.Z;
                // определение максимальной реакции усилия кН
                pass.PassRollingParameters.RSPP = Math.Max(pass.PassRollingParameters.P1SPP * pass.Pass.SUMX / pass.RollingStand.AKL, pass.PassRollingParameters.P1SPP - pass.PassRollingParameters.P1SPP * pass.Pass.SUMX / pass.RollingStand.AKL);
                pass.PassRollingParameters.RSPZ = Math.Max(pass.PassRollingParameters.P1SPZ * pass.Pass.SUMX / pass.RollingStand.AKL, pass.PassRollingParameters.P1SPZ - pass.PassRollingParameters.P1SPZ * pass.Pass.SUMX / pass.RollingStand.AKL);
                // коэфф. загрузки по усилию
                pass.PassRollingParameters.KPSPP = pass.PassRollingParameters.RSPP / pass.RollingStand.PDOP;
                pass.PassRollingParameters.KPSPZ = pass.PassRollingParameters.RSPZ / pass.RollingStand.PDOP;
                // момент трения в шейках кН*М
                pass.PassRollingParameters.MTRSPP = pass.PassRollingParameters.P1SPP * pass.RollingStand.FPOD * pass.RollingStand.DSH / 1000;
                pass.PassRollingParameters.MTRSPZ = pass.PassRollingParameters.P1SPZ * pass.RollingStand.FPOD * pass.RollingStand.DSH / 1000;
                // момент деформации с учетом числа ниток кН*М
                pass.PassRollingParameters.MDSPP = 2 * pass.PassRollingParameters.P1SPP * pass.PassRollingParameters.LSP * pass.Pass.PSI / 1000;
                pass.PassRollingParameters.MDSPZ = 2 * pass.PassRollingParameters.P1SPZ * pass.PassRollingParameters.LSP * pass.Pass.PSI / 1000;
                // крутящий момент на валках kN*M
                pass.PassRollingParameters.MPRSPP = pass.PassRollingParameters.MDSPP + pass.PassRollingParameters.MTRSPP;
                pass.PassRollingParameters.MPRSPZ = pass.PassRollingParameters.MDSPZ + pass.PassRollingParameters.MTRSPZ;
                // коэфф. загрузки по моменту
                pass.PassRollingParameters.KMSPP = pass.PassRollingParameters.MPRSPP / pass.RollingStand.MDOP;
                pass.PassRollingParameters.KMSPZ = pass.PassRollingParameters.MPRSPZ / pass.RollingStand.MDOP;
                // мощность на рабочих валках кВт
                pass.PassRollingParameters.NPSPP = pass.PassRollingParameters.MPRSPP * pass.PassRollingParameters.NR / 9.549;
                pass.PassRollingParameters.NPSPZ = pass.PassRollingParameters.MPRSPZ * pass.PassRollingParameters.NR / 9.549;
                // затраты энергии кДж/тонна
                pass.PassRollingParameters.WWSPP = 0.131 * 1000000 * (pass.PassRollingParameters.MPRSPP / 9.807) * pass.PassRollingParameters.NR * pass.PassRollingParameters.TM / (pass.Pass.W * pass.PassRollingParameters.LP);
                pass.PassRollingParameters.WWSPZ = 0.131 * 1000000 * (pass.PassRollingParameters.MPRSPZ / 9.807) * pass.PassRollingParameters.NR * pass.PassRollingParameters.TM / (pass.Pass.W * pass.PassRollingParameters.LP);
                // тоже в кВт*ч/тонна
                pass.PassRollingParameters.WWSPP /= 3600;
                pass.PassRollingParameters.WWSPZ /= 3600;
                // крутящий момент, приведенный к двигателю кН*М
                pass.PassRollingParameters.MISPP = pass.PassRollingParameters.MPRSPP / (pass.RollingStand.IR * pass.RollingStand.ETA);
                pass.PassRollingParameters.MISPZ = pass.PassRollingParameters.MPRSPZ / (pass.RollingStand.IR * pass.RollingStand.ETA);
                
                // загрузка электродвигателя                
                var mispp = _context.Passes.Select(p => p.PassRollingParameters.MISPP).ToList();
                var misumSpp = _context.Passes.Select(p => p.PassRollingParameters.MISUMSPP).ToList();
                var kdvspp = _context.Passes.Select(p => p.PassRollingParameters.KDVSPP).ToList();

                var mispz = _context.Passes.Select(p => p.PassRollingParameters.MISPZ).ToList();
                var misumSpz = _context.Passes.Select(p => p.PassRollingParameters.MISUMSPZ).ToList();
                var kdvspz = _context.Passes.Select(p => p.PassRollingParameters.KDVSPZ).ToList();

                Dvigat(mispp, misumSpp, kdvspp);
                Dvigat(mispz, misumSpz, kdvspz);
            }
        }

        public void Peresilka(int i) // пересылка H1 и B1 в H0 и B0 c учетом катновки после их определения в модуле forma
        {
            var pass = _context.Passes[i];
            if (i < _context.GlobalRollingParameters.NPR) // Проверка на выход за границы
            {
                var nextPass = _context.Passes[i + 1];
                switch (pass.Pass.SchemaCaliber)
                {
                    case 2: // круг
                        nextPass.Pass.H0 = pass.Pass.H1;
                        nextPass.Pass.B0 = pass.Pass.B1;
                        break;
                    case 4: // овал
                    case 5: // плоский овал
                    case 6: // шестиугольник
                    case 7: // ромб
                    case 9: // ребровой овал
                        nextPass.Pass.H0 = pass.Pass.B1;
                        nextPass.Pass.B0 = pass.Pass.H1;
                        break;
                    case 3: // квадрат
                        if (nextPass.Pass.Schema == 37)
                        {
                            nextPass.Pass.H0 = pass.Pass.H1;
                            nextPass.Pass.B0 = pass.Pass.B1;
                        }
                        else
                        {
                            double c = (pass.Pass.H1 + 0.83 * pass.Pass.R) / Math.Sqrt(2);
                            nextPass.Pass.H0 = c;
                            nextPass.Pass.B0 = c;
                        }
                        break;
                    case 8: // ящичный калибр
                    case 0: // гладкая бочка
                        if (pass.Pass.H1 < nextPass.Pass.H1)
                        {
                            nextPass.Pass.H0 = pass.Pass.B1;
                            nextPass.Pass.B0 = pass.Pass.H1;
                        }
                        else
                        {
                            nextPass.Pass.H0 = pass.Pass.H1;
                            nextPass.Pass.B0 = pass.Pass.B1;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void Forma(int i, int Schema) // расчет формоизменения при извенстных размерах калибра,
        {
            var pass = _context.Passes[i];
            // с учетом марки стали
            // относительные параметры калибров
            pass.PassRollingParameters.KOBJ = pass.Pass.H0 / pass.Pass.H1; // коэфф.обжатия
            pass.PassRollingParameters.A = pass.PassRollingParameters.DD / pass.Pass.H1; // приведен.диаметр
            if (pass.PassRollingParameters.BK < 0.001)
                pass.PassRollingParameters.AK = 1;
            else
                pass.PassRollingParameters.AK = pass.PassRollingParameters.BK / pass.PassRollingParameters.HG; // отношение осей калибра
            pass.PassRollingParameters.A0 = pass.Pass.H0 / pass.Pass.B0; // отношение осей исходной полосы
            if (pass.Pass.SchemaCaliber == 8) // ящичный калибр
                pass.PassRollingParameters.TANFI = (pass.PassRollingParameters.BK - pass.Pass.BD) / pass.Pass.H1; // выпуск калибра
                                                                                                 // Изменения внесены для работы с одним проходом
                                                                                                 // if (i==1)
                                                                                                 // DEL0[i]=1;
                                                                                                 // else
            if (i > 1) // Проверка индекса
                pass.PassRollingParameters.DEL0 = _context.Passes[i - 1].Pass.B1 / _context.Passes[i - 1].PassRollingParameters.BK;
            if (pass.PassRollingParameters.DEL0 > 1) // переполнение калибра
                pass.PassRollingParameters.DEL0 = 1;
            // расчет продолжается
            pass.PassRollingParameters.E = Stepdef(i, Schema); // степень деформации
                                                               // ориентировочные параметры прокатки
            if (i == 1)
            {
                double V0 = _context.InitialParameters.VK / (_context.InitialParameters.W0 / _context.Passes[_context.GlobalRollingParameters.NPR].Pass.W); // скорость входа в стан
                pass.PassRollingParameters.TP = _context.InitialParameters.TAU + _context.InitialParameters.L0 / (2 * V0); // время движения середины полосы до первой клети
            }
            else
            {
                double V0 = _context.Passes[i - 1].PassRollingParameters.V1; // скорость входа в клеть i
                pass.PassRollingParameters.TP = _context.Passes[i - 1].RollingStand.LKL / V0; // время движения середины полосы между клетями
            }
            if (_context.GlobalRollingParameters.K13 == 9) // задана опытная температура
                pass.PassRollingParameters.TSR = pass.Pass.TOP;
            else // расчет температуры
            {
                if (i == 1)
                    pass.PassRollingParameters.TSR = Temp(_context.InitialParameters.P0, pass.PassRollingParameters.TP, _context.InitialParameters.W0, 0, _context.InitialParameters.T0);
                else
                {
                    var prevPass = _context.Passes[i - 1];
                    prevPass.PassRollingParameters.DTDSR = 0.183 * prevPass.PassRollingParameters.SIGSSR * Math.Log(prevPass.PassRollingParameters.KVIT);
                    pass.PassRollingParameters.TSR = Temp(prevPass.PassRollingParameters.PER, pass.PassRollingParameters.TP, prevPass.Pass.W, prevPass.PassRollingParameters.DTDSR, prevPass.PassRollingParameters.TSR);
                }
            }
            // выбор показателя трения по средней температуре
            if ((pass.Pass.Schema == 37) || (pass.Pass.Schema == 73) || (pass.Pass.Schema == 77))
            {
                pass.PassRollingParameters.PSITR = 0.5;
                if (pass.PassRollingParameters.TSR < 1100)
                    pass.PassRollingParameters.PSITR = 0.6;
                if (pass.PassRollingParameters.TSR < 1000)
                    pass.PassRollingParameters.PSITR = 0.75;
            }
            else
            {
                pass.PassRollingParameters.PSITR = 0.6;
                if (pass.PassRollingParameters.TSR < 1200)
                    pass.PassRollingParameters.PSITR = 0.7;
                if (pass.PassRollingParameters.TSR < 1100)
                    pass.PassRollingParameters.PSITR = 0.8;
                if (pass.PassRollingParameters.TSR < 1000)
                    pass.PassRollingParameters.PSITR = 0.9;
                if ((pass.Pass.SchemaCaliber == 8) || (pass.Pass.SchemaCaliber == 1) || (pass.Pass.SchemaCaliber == 0))
                    pass.PassRollingParameters.PSITR = pass.PassRollingParameters.PSITR - 0.1;
            }
            if (pass.PassRollingParameters.TSR < 900)
                pass.PassRollingParameters.PSITR = 1;
            // расчет коэфф.уширения базовой стали
            switch (pass.Pass.SchemaCaliber)
            {
                case 0:
                case 1: // гладкая бочка
                    if (pass.Pass.SchemaInput == 2) // круг
                        pass.PassRollingParameters.BETAR = Spred(0.179, 1.357, 0.291, 0, 0, 0, 0.511, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 1) // прямоугольник
                        pass.PassRollingParameters.BETAR = Spred(0.0714, 0.862, 0.555, 0.763, 0, 0, 0.455, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 2: // круглый калибр
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.BETAR = Spred(0.386, 1.163, 0.402, -2.171, 0, -1.324, 0.616, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 5) // плоский овал
                        pass.PassRollingParameters.BETAR = Spred(0.693, 1.286, 0.368, -1.052, 0, -2.231, 0.629, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 3: // квадратный калибр
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.BETAR = Spred(2.242, 1.151, 0.352, -2.234, 0, -1.647, 1.137, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.BETAR = Spred(0.360, 0.658, 0.202, -0.467, 0, -3.316, 0.494, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    // BETAR[i]=spred(0.948,1.203,0.368,-0.852,0,-3.45,0.659,0,KOBJ[i],A[i],A0[i],AK[i],DEL0[i],PSITR[i],TANFI[i]);
                    // BETAR[i]=spred(1.322,1.203,0.368,-0.852,0,-7.43,0.659,0,KOBJ[i],A[i],A0[i],AK[i],DEL0[i],PSITR[i],TANFI[i]);

                    if (pass.Pass.SchemaInput == 7) // ромб
                        pass.PassRollingParameters.BETAR = Spred(0.972, 2.01, 0.665, -2.458, 0, -1.3, 0.7, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 4: // овальный калибр
                    if ((pass.Pass.SchemaInput == 1) || (pass.Pass.SchemaInput == 8)) // квадрат или ящичный квадрат
                        pass.PassRollingParameters.BETAR = Spred(0.377, 0.507, 0.316, 0, -0.405, 0, 1.136, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 2) // круг
                        pass.PassRollingParameters.BETAR = Spred(0.227, 1.563, 0.591, 0, -0.852, 0, 0.587, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.BETAR = Spred(0.405, 1.163, 0.403, -2.171, -0.789, -1.324, 0.616, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 9) // ребровой овал
                        pass.PassRollingParameters.BETAR = Spred(1.623, 2.272, 0.761, -0.582, -3.064, 0, 0.486, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 5: // плоский овальный калибр
                    if (pass.Pass.SchemaInput == 1) // квадрат
                        pass.PassRollingParameters.BETAR = Spred(0.134, 0.717, 0.474, 0, -0.507, 0, 0.357, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 6: // шестиугольный калибр
                    if (pass.Pass.SchemaInput == 1) // квадрат
                        pass.PassRollingParameters.BETAR = Spred(2.075, 1.848, 0.815, 0, -3.453, 0, 0.659, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 7: // ромбический калибр
                    if (pass.Pass.SchemaInput == 3) // квадрат
                        pass.PassRollingParameters.BETAR = Spred(3.09, 2.07, 0.5, 0, -4.85, -4.865, 1.543, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    if (pass.Pass.SchemaInput == 7) // ромб
                        pass.PassRollingParameters.BETAR = Spred(0.506, 1.876, 0.895, -2.22, -2.22, -2.73, 0.587, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 8: // ящичный калибр
                    if ((pass.Pass.SchemaInput == 8) || (pass.Pass.SchemaInput == 1)) // прямоугольник из ящичного или гл.бочки
                        pass.PassRollingParameters.BETAR = Spred(0.0714, 0.862, 0.746, 0.763, 0, 0, 0.16, 0.362, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 9: // ребровой овальный калибр
                    if (pass.Pass.SchemaInput == 4) // овал
                        pass.PassRollingParameters.BETAR = Spred(0.575, 1.163, 0.402, -2.171, -4.265, -1.324, 0.616, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
                case 10:
                    if (pass.Pass.SchemaInput == 6) // шестиугольник
                        pass.PassRollingParameters.BETAR = Spred(0.3, 1.203, 0.368, -0.852, 0, -3.45, 0.659, 0, pass.PassRollingParameters.KOBJ, pass.PassRollingParameters.A, pass.PassRollingParameters.A0, pass.PassRollingParameters.AK, pass.PassRollingParameters.DEL0, pass.PassRollingParameters.PSITR, pass.PassRollingParameters.TANFI);
                    break;
            }
            // ширина полосы
            pass.Pass.B1 = pass.Pass.B0 * pass.PassRollingParameters.BETAR;
            double b;
            do
            {
                double sb = 0; // сопротивление деформации базовой стали
                b = pass.Pass.B1;
                // площадь поперечного сечения и коэфф.вытяжки
                pass.PassRollingParameters.A1 = b / pass.Pass.H1;
                pass.PassRollingParameters.DEL1 = b / pass.PassRollingParameters.BK;
                if (pass.PassRollingParameters.DEL1 > 1) // переполнение
                    pass.PassRollingParameters.DEL1 = 1;
                pass.Pass.W = Omega(i);
                if (i == 1)
                    pass.PassRollingParameters.KVIT = _context.InitialParameters.W0 / pass.Pass.W;
                else
                    pass.PassRollingParameters.KVIT = _context.Passes[i - 1].Pass.W / pass.Pass.W;
                // расчет скорости прокатки и деформации в проходе (ориентировачно)
                if (i == 1)
                    pass.PassRollingParameters.V1 = _context.InitialParameters.VK * pass.PassRollingParameters.KVIT / (_context.InitialParameters.W0 / _context.Passes[_context.GlobalRollingParameters.NPR].Pass.W);
                else
                    pass.PassRollingParameters.V1 = _context.Passes[i - 1].PassRollingParameters.V1 * pass.PassRollingParameters.KVIT;
                pass.PassRollingParameters.DK = (pass.RollingStand.DB + pass.Pass.S) - pass.Pass.W / b;
                pass.PassRollingParameters.NR = 60000 * pass.PassRollingParameters.V1 / (Math.PI * pass.PassRollingParameters.DK);
                if (_context.GlobalRollingParameters.K12 == 1) // заданные обороты
                {
                    pass.PassRollingParameters.NR = pass.RollingStand.NZ;
                    pass.PassRollingParameters.V1 = Math.PI * pass.PassRollingParameters.DK * pass.PassRollingParameters.NR / 60000;
                }
                pass.PassRollingParameters.KSI = 0.105 * pass.PassRollingParameters.NR * Math.Sqrt(pass.PassRollingParameters.E * pass.PassRollingParameters.DK / (2 * pass.Pass.H0));
                // среднее сопротивление деформации и поправочный коэфф.уширения
                if (_context.GlobalRollingParameters.K11 == 1) // по Зюзину
                    pass.PassRollingParameters.SIGSSR = 9.807 * Sig1(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.TSR);
                if (_context.GlobalRollingParameters.K11 == 2) // по Тюленеву
                    pass.PassRollingParameters.SIGSSR = 9.807 * Sig2(_context.Steel.K5, _context.Steel.K6, _context.Steel.K7, _context.Steel.K8, _context.Steel.K9, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.TSR);
                if (_context.GlobalRollingParameters.K11 == 3) // по методу кафедры ОМД
                    pass.PassRollingParameters.SIGSSR = 9.807 * Sig2(_context.Steel.K1, _context.Steel.K2, _context.Steel.K3, _context.Steel.K4, _context.Steel.K5, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.TSR);
                // базовое сопротивление деформации
                if ((_context.GlobalRollingParameters.K11 == 1) || (_context.GlobalRollingParameters.K11 == 3)) // по Зюзину
                    sb = 9.807 * Sig1(130, 0.252, 0.143, 0.0025, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.TSR);
                if (_context.GlobalRollingParameters.K11 == 2) // по Тюленеву
                    sb = 9.807 * Sig2(1.41, 9.07, 0.124, 0.167, -2.54, pass.PassRollingParameters.E, (pass.PassRollingParameters.KSI > 120) ? 120 : pass.PassRollingParameters.KSI, pass.PassRollingParameters.TSR);
                // расчет поправочного коэфф. массивы по переднему концу используюся вместо массивов по середине
                if ((pass.PassRollingParameters.SIGSSR / sb) > 1.001)
                    pass.PassRollingParameters.KBETAP = 1 + 0.6 * (Math.Exp(0.544 * Math.Log((pass.PassRollingParameters.SIGSSR / sb) - 1)));
                else
                    pass.PassRollingParameters.KBETAP = 1;
                // коэфф.уширения с учетом марки стали
                pass.PassRollingParameters.BETASTP = 1 + (pass.PassRollingParameters.BETAR - 1) * pass.PassRollingParameters.KBETAP;
                // ширина полосы после прокатки
                pass.Pass.B1 = pass.Pass.B0 * pass.PassRollingParameters.BETASTP;
                switch (pass.Pass.SchemaCaliber)
                {
                    case 0: // гладкая бочка
                    case 1: // гладкая бочка
                        pass.PassRollingParameters.PER = 2 * (pass.Pass.H1 + pass.Pass.B1);
                        break;
                    case 2: // круглый калибр
                        pass.PassRollingParameters.PER = Math.PI * pass.Pass.H1;
                        break;
                    case 3: // квадратный калибр
                        pass.PassRollingParameters.PER = 2.828 * pass.PassRollingParameters.BK;
                        break;
                    case 4: // овальный калибр
                        pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.B1 * pass.Pass.B1 + 4 * (pass.Pass.H1 * pass.Pass.H1) / 3);
                        break;
                    case 5: // плоский овальный калибр
                        pass.PassRollingParameters.PER = Math.PI * pass.Pass.H1 + 2 * (pass.Pass.B1 - pass.Pass.H1);
                        break;
                    case 6: // шестиугольный
                    case 8: // ящичный
                        if (pass.Pass.SchemaInput == 6) // шестигранный калибр
                            pass.PassRollingParameters.PER = 3 * pass.Pass.H1;
                        else
                            pass.PassRollingParameters.PER = 2 * (pass.Pass.BD + pass.Pass.H1) / Math.Cos(Math.Atan((pass.Pass.BVR - pass.Pass.BD) / (pass.Pass.H1 - pass.Pass.S)));
                        break;
                    case 7: // ромбический
                        pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.H1 * pass.Pass.H1 + pass.Pass.B1 * pass.Pass.B1);
                        break;
                    case 9: // ребровой овальный
                        pass.PassRollingParameters.PER = 2 * Math.Sqrt(pass.Pass.H1 * pass.Pass.H1 + 4 * pass.Pass.B1 * pass.Pass.B1 / 3);
                        break;
                    default:
                        break;
                }
            } while ((Math.Abs(b - pass.Pass.B1) / b) > 0.005);
            Peresilka(i); // ,H1[i],B1[i]);,Schema[i]
        }

        public void Zazor() // определение установочного зазора SU с учетом упругой деформации клети
        {
            // C - коэфф. жесткости, кН /мм
            for (int i = 1; i <= _context.GlobalRollingParameters.NPR; i++)
            {
                var pass = _context.Passes[i];
                if (pass.RollingStand.C > 0.001)
                {
                    pass.PassRollingParameters.FKLP = pass.PassRollingParameters.P1P / pass.RollingStand.C; // упругая деформация по переднему концу
                    pass.PassRollingParameters.FKLZ = pass.PassRollingParameters.P1Z / pass.RollingStand.C; // упругая деформация по заднему концу
                    pass.PassRollingParameters.FKL = (pass.PassRollingParameters.FKLP + pass.PassRollingParameters.FKLZ) / 2; // среднее значение упругой деформации
                                                                           // установочный зазор через HVR==(H1-S)/2
                    pass.PassRollingParameters.SUP = pass.Pass.H1 - 2 * pass.PassRollingParameters.HVR - pass.PassRollingParameters.FKLP; // уст.зазор по переднему концу
                    pass.PassRollingParameters.SUZ = pass.Pass.H1 - 2 * pass.PassRollingParameters.HVR - pass.PassRollingParameters.FKLZ; // уст.зазор по заднему концу
                    pass.PassRollingParameters.SU = pass.Pass.H1 - 2 * pass.PassRollingParameters.HVR - pass.PassRollingParameters.FKL; // среднее значение уст.зазора
                }
                else
                    ; // отсутствует значение коэфф. жесткости клети
            }
        }

        // Определение максимальной скорости прокатки по частоте вращения валков
        public void SkorostMax()
        {
            var lastPass = _context.Passes[_context.GlobalRollingParameters.NPR];
            lastPass.PassRollingParameters.VRMAX = lastPass.PassRollingParameters.VMAX; // максимальная конечная скорость по валкам
            _context.Passes[1].PassRollingParameters.VRMAX = lastPass.PassRollingParameters.VRMAX * _context.Passes[1].PassRollingParameters.KVIT / (_context.InitialParameters.W0 / lastPass.Pass.W); // скорость в первой клети
            if (_context.Passes[1].PassRollingParameters.VRMAX > _context.Passes[1].PassRollingParameters.VMAX)
            {
                _context.Passes[1].PassRollingParameters.VRMAX = _context.Passes[1].PassRollingParameters.VMAX;
                lastPass.PassRollingParameters.VRMAX = _context.Passes[1].PassRollingParameters.VRMAX * (_context.InitialParameters.W0 / lastPass.Pass.W) / _context.Passes[1].PassRollingParameters.KVIT;
            }
            for (int i = 1; i < _context.GlobalRollingParameters.NPR - 1; i++)
            {
                var currentPass = _context.Passes[i];
                var nextPass = _context.Passes[i + 1];
                nextPass.PassRollingParameters.VRMAX = currentPass.PassRollingParameters.VRMAX * nextPass.PassRollingParameters.KVIT;
                if (nextPass.PassRollingParameters.VRMAX > nextPass.PassRollingParameters.VMAX)
                {
                    nextPass.PassRollingParameters.VRMAX = nextPass.PassRollingParameters.VMAX;
                    lastPass.PassRollingParameters.VRMAX = nextPass.PassRollingParameters.VRMAX * nextPass.Pass.W / lastPass.Pass.W;
                }
            } // VRMAX[NPR] - максимальная рабочая скорость прокатки по максимальной скорости валков
            for (int i = _context.GlobalRollingParameters.NPR - 1; i >= 1; i--)
            {
                var currentPass = _context.Passes[i];
                var nextPass = _context.Passes[i + 1];
                currentPass.PassRollingParameters.VRMAX = nextPass.PassRollingParameters.VRMAX / nextPass.PassRollingParameters.KVIT;
            }
        }

    }
}
