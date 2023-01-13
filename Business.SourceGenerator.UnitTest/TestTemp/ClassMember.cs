
namespace MyCode
{
    public partial class ClassMember
    {
        const string ADEF = "ADEF!!!";

        public string A { get; set; } = ADEF;

        public System.Collections.Generic.Dictionary<string, int?>? B { get; set; }

        public System.DateTime? C { get; set; } = default;

        public object? D { get; set; }

        public dynamic? E { get; set; }

        public System.Action<int?>? F;

        public string A1 = "a1def!!!";

        public int? B1 = 333;

        public System.DateTime? C1 = System.DateTime.Now;

        public object? D1;

        public dynamic? E1;

        public System.Action<int?> F1;

        //========================================================//

        public static string A2 { get; set; }

        public static int? B2 { get; set; }

        public static System.DateTime? C2 { get; set; }

        public static object? D2 { get; set; }

        public static dynamic? E2 { get; set; }

        public static System.Action<int?> F2 { get; set; }

        public static string A3;

        public static int? B3;

        public static System.DateTime? C3;

        public static object? D3;

        public static dynamic? E3;

        public static System.Action<int?> F3;

        public ClassMember()
        {
            
        }

        public ClassMember(string a, System.DateTime? c, object d, dynamic e, System.Action<int?> f, string a1, int? b1, System.DateTime? c1, object d1, dynamic e1, System.Action<int?> f1)
        {
            A = a;
            //B = b;
            C = c;
            D = d;
            E = e;
            F = f;
            A1 = a1;
            B1 = b1;
            C1 = c1;
            D1 = d1;
            E1 = e1;
            F1 = f1;
        }
    }
}