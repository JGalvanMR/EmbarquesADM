using System;
using Xamarin.Forms;
using System.Data;
using System.Data.SqlClient;

namespace EmbarquesADM
{
    public partial class LoginPage : ContentPage
    {

        public static string cadenaConexion = "Persist Security Info=False;user id=sa; password=Gabira1;Initial Catalog =GAB_Irapuato; server=tcp:189.206.160.206,2352; MultipleActiveResultSets=true; Connect Timeout = 130";
        SqlCommand cmnd = new SqlCommand();
        SqlDataReader reader;
        SqlDataAdapter da;
        SqlConnection thisConnection;
        string query = "";
        string usuario = "";
        public static DataTable responsables = new DataTable("responsables");
        DataSet ds = new DataSet();


        public LoginPage()
        {
            thisConnection = new SqlConnection(cadenaConexion);
            InitializeComponent();
        }

        async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var user = new User
            {
                Username = usernameEntry.Text,
                Password = passwordEntry.Text
            };

            var isValid = AreCredentialsCorrect(user);
            if (isValid)
            {
                App.IsUserLoggedIn = true;
                Navigation.InsertPageBefore(new MainPage(usuario), this);
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error!", "La contraseña o el usuario es incorrecto", "OK");
            }
        }

        bool AreCredentialsCorrect(User user)
        {
            thisConnection.Open();
            //Buscar el numero del que parte si es campo o proceso
            cmnd = thisConnection.CreateCommand();


            query = "SELECT * FROM tb_cat_userEmbADM WHERE estado = 'A'";
            da = new SqlDataAdapter(query, thisConnection);
            da.Fill(ds, "responsables");
            responsables = ds.Tables["responsables"];
            thisConnection.Close();


            bool logeado = false;

            for (int i = 0; i < responsables.Rows.Count; i++)
            {

                if (user.Username == responsables.Rows[i]["Usuario"].ToString().Trim() && user.Password == responsables.Rows[i]["password"].ToString())
                {
                    logeado = true;
                    usuario = responsables.Rows[i]["Nombre"].ToString().Trim();
                }
            }

            thisConnection.Open();
            string cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                        "VALUES(GETDATE(),'" + usuario + "','Ingreso a sistema EMBARQUES ADM', 'I')";
            SqlCommand cmd = new SqlCommand(cadena, thisConnection);
            cmd.ExecuteNonQuery();
            thisConnection.Close();


            //return user.Username == Constants.Username && user.Password == Constants.Password;



            return logeado;
            //return user.Username == Constants.Username && user.Password == Constants.Password;
        }
    }
}
