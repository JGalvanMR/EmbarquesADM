using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Net;

namespace EmbarquesADM
{

    public partial class MainPage : ContentPage
    {
        string usuariologueado = "";
        SqlConnection thisConnection = new SqlConnection(LoginPage.cadenaConexion);
        SqlDataAdapter da;
        DataSet ds = new DataSet();
        SqlCommand cmnd = new SqlCommand();
        SqlCommand cmnd1 = new SqlCommand();
        SqlDataReader reader1;
        DataTable Inven = new DataTable();
        DataTable Semanas = new DataTable();


        public MainPage(string usuario)
        {
            InitializeComponent();
            NombredeUsuario.Text = "Bienvenido. " + usuario;
            usuariologueado = usuario;

            CreaTable();
            Int32 anio = DateTime.Now.Year;
            thisConnection.Open();
            String Cadena = "SELECT semana, ano, fecha1, fecha2, generado, indica FROM tb_cat_semanas Where ano >= '" + (anio - 1).ToString() + "' and ano <= '" + (anio + 1).ToString() + "'";
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "Semanas");

            Semanas = ds.Tables["Semanas"];
            thisConnection.Close();
        }

        async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            App.IsUserLoggedIn = false;
            Navigation.InsertPageBefore(new LoginPage(), this);
            await Navigation.PopAsync();
        }

        async void OnContinuarClicked(object sender, EventArgs e)
        {
            string opcion = (string)picker.ItemsSource[picker.SelectedIndex];
            if (opcion == "Abrir Orden")
            {
                string ordenabrir = await InputBox(this.Navigation);

                string Tipoped = "NAL";
                string tb_tabla = "tb_mstr_pedidos_nal";


                if (ordenabrir.Length > 0)
                {
                    if (Convert.ToInt32(ordenabrir) < 300000)
                    {
                        Tipoped = "EXP";
                        tb_tabla = "tb_mstr_pedidos_exp";

                    }
                }

                thisConnection.Open();
                DateTime FechaHoy = DateTime.Now;

                string Cadena = "Select pdn_fecha from " + tb_tabla + " Where pdn_folio = '" + ordenabrir + "' AND pdn_estatus != 'C'";
                SqlCommand cmdxi = new SqlCommand(Cadena, thisConnection);
                DateTime FechaPedido = Convert.ToDateTime(cmdxi.ExecuteScalar());

                TimeSpan tspan = FechaHoy - FechaPedido;//Se calcula la cantidad de dias que han pasado desde que el pedido fue solicitado, a la fecha de HOY se le resta la fecha del pedido

                int dias = tspan.Days;

                thisConnection.Close();
                if (dias > 15)//Si han pasado mas de 15 dias que el pedido fue realizado el sistema arrojara un ERROR del usuario
                {
                    await DisplayAlert("Error!", "El pedido es Mayor a 15 Dias", "OK");
                }
                else
                {
                    string pedidoorigen = "";
                    int Afectados = 0;

                    thisConnection.Open();
                    Cadena = "Select pdn_pedorigen from " + tb_tabla + " Where pdn_folio = '" + ordenabrir + "' AND pdn_estatus != 'C'";
                    cmdxi = new SqlCommand(Cadena, thisConnection);
                    pedidoorigen = Convert.ToString(cmdxi.ExecuteScalar());
                    thisConnection.Close();

                    if (pedidoorigen == "0")//Si el origen del pedido es igual a 0 el sistema le asignara como origen del pedido el numero de folio del pedido, es decir la orden que se desea abrir.
                    {
                        pedidoorigen = ordenabrir;
                    }

                    thisConnection.Open();
                    string cadena = "UPDATE tb_mstr_trailer SET horafin = '--:--', Guardar = 'N' WHERE " +
                                "pdn_folio = '" + pedidoorigen + "'";
                    SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                    //cmd.ExecuteNonQuery();
                    Afectados = Convert.ToInt32(cmd.ExecuteNonQuery());
                    thisConnection.Close();


                    if (Tipoped == "EXP")//Si el tipo del producto es de EXPORTACION ("EXP") el sistema añadira un 0 al principio del numero de la orden a abrir
                    {
                        ordenabrir = "0" + ordenabrir;
                    }

                    //Fecha: Viernes 09 de Junio de 2023 
                    //Realiza: Jesus Galvan jgalvan@mrlucky.com.mx
                    //Descripción el Cambio: se comenta la linea de codigo 126 y 131 para se añade la 127 debido a un error en la consulta para obtener el numero del trailer (no_trailer)
                    if (Afectados == 0)
                    {
                        string no_trailer = "";
                        string hora_trailer = "";
                        thisConnection.Open();
                        //Cadena = "Select no_trailer from tb_mstr_embarque Where pdn_folio = '" + ordenabrir + "' AND pdn_estatus != 'C'";
                        Cadena = "Select no_trailer from tb_mstr_embarque Where emb_folio = '" + ordenabrir + "' AND sts != 'C'";
                        cmdxi = new SqlCommand(Cadena, thisConnection);
                        no_trailer = Convert.ToString(cmdxi.ExecuteScalar());

                        //Cadena = "Select hora_trailer from tb_mstr_embarque Where pdn_folio = '" + ordenabrir + "' AND pdn_status != 'C'";
                        Cadena = "Select hora_trailer from tb_mstr_embarque Where emb_folio = '" + ordenabrir + "' AND sts != 'C'";
                        cmdxi = new SqlCommand(Cadena, thisConnection);
                        hora_trailer = Convert.ToString(cmdxi.ExecuteScalar());

                        if (no_trailer != "" && hora_trailer != "")//si el número del trailer (placas) y hora del trailer son vacias se actualizan
                        {
                            cadena = "UPDATE tb_mstr_trailer SET horafin = '--:--', Guardar = 'N' WHERE " +
                                "no_trailer = '" + pedidoorigen + "' AND hora_trailer = '" + hora_trailer + "'";
                            cmd = new SqlCommand(cadena, thisConnection);
                            cmd.ExecuteNonQuery();
                        }
                        thisConnection.Close();
                    }

                    thisConnection.Open();
                    cadena = "UPDATE tb_mstr_embarque SET sts = 'C', hora_fin = '--:--' WHERE " +
                                "emb_folio = '" + ordenabrir + "' AND emb_tipo = '" + Tipoped + "'";
                    cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    await DisplayAlert("Exito!", "El pedido ha sido Abierto Correctamente", "OK");

                    thisConnection.Open();
                    cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                                "VALUES(GETDATE(),'" + usuariologueado + "','Orden Abierta " + ordenabrir.Trim() + "', 'OA')";
                    cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    picker.SelectedIndex = -1;
                }
            }
            else if (opcion == "Desvincular Trailer a Orden")
            {
                string permiso = "0";
                if (usuariologueado != "DANIEL ANTONIO MUÑOZ HERNANDEZ" && usuariologueado != "RICARDO CORTES")
                {
                    thisConnection.Open();
                    string cadenav = "SELECT CASE When (SELECT DATENAME(dw, GETDATE())) = 'Domingo' THEN '1' WHEN (SELECT DATENAME(dw, GETDATE())) = 'Sábado' AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'13:30:00', 108) HoraServidor)) THEN '2' WHEN ((SELECT DATENAME(dw, GETDATE())) NOT IN ('Domingo', 'Sábado') AND (SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'17:30:00', 108) HoraServidor)) THEN '2' ELSE '1' END";
                    SqlCommand cmdv = new SqlCommand(cadenav, thisConnection);
                    permiso = Convert.ToString(cmdv.ExecuteScalar());
                    thisConnection.Close();
                }

                if (permiso == "2")
                {
                    await DisplayAlert("Operacion Denegada!", "La operacion actual ha sido denegada debido a que esta activo el area de sistemas para realizar dicho movimiento, Favor de Notificar a Vigilancia el Levantamiento del Ticket", "OK");

                }
                else
                {
                    string ordendesvincular = await InputBox(this.Navigation);

                    string Tipoped = "NAL";
                    string tb_tabla = "tb_mstr_pedidos_nal";


                    if (ordendesvincular.Length > 0)
                    {
                        if (Convert.ToInt32(ordendesvincular) < 300000)
                        {
                            Tipoped = "EXP";
                            tb_tabla = "tb_mstr_pedidos_exp";

                        }
                    }

                    thisConnection.Open();
                    DateTime FechaHoy = DateTime.Now;

                    string Cadena = "Select pdn_fecha from " + tb_tabla + " Where pdn_folio = '" + ordendesvincular + "' AND pdn_estatus != 'C'";
                    SqlCommand cmdxi = new SqlCommand(Cadena, thisConnection);
                    DateTime FechaPedido = Convert.ToDateTime(cmdxi.ExecuteScalar());

                    TimeSpan tspan = FechaHoy - FechaPedido;

                    int dias = tspan.Days;

                    thisConnection.Close();
                    if (dias > 15)
                    {
                        await DisplayAlert("Error!", "El pedido es Mayor a 15 Dias", "OK");
                    }
                    else
                    {
                        if (Tipoped == "EXP")
                        {
                            ordendesvincular = "0" + ordendesvincular;
                        }

                        thisConnection.Open();

                        Cadena = "Select emb_folio from tb_mstr_embarque Where emb_folio = '" + ordendesvincular + "'";
                        cmdxi = new SqlCommand(Cadena, thisConnection);
                        string emb_folio = Convert.ToString(cmdxi.ExecuteScalar());
                        thisConnection.Close();

                        if (emb_folio.Length > 0)
                        {
                            await DisplayAlert("Error!", "El pedido Cuenta con Embarque, Favor de Cancelarlo", "OK");
                        }
                        else
                        {
                            thisConnection.Open();
                            string cadena = "DELETE tb_mstr_trailer WHERE pdn_folio = '" + Convert.ToInt32(ordendesvincular) + "'";
                            SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                            cmd.ExecuteNonQuery();
                            thisConnection.Close();

                            await DisplayAlert("Exito!", "La orden se ha desvinculado", "OK");

                            thisConnection.Open();
                            cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                                        "VALUES(GETDATE(),'" + usuariologueado + "','Orden Desvinculada " + ordendesvincular.Trim() + "', 'OD')";
                            cmd = new SqlCommand(cadena, thisConnection);
                            cmd.ExecuteNonQuery();
                            thisConnection.Close();
                            picker.SelectedIndex = -1;
                        }

                    }

                }
            }
            else if (opcion == "Cerrar Sesion via Remota")
            {
                string SesionCerrar = await SelectedBox(this.Navigation);

                thisConnection.Open();
                string cadena = "UPDATE tb_det_acceso_celulares SET estado = 'T' WHERE " +
                            "nom_usu = '" + SesionCerrar.Trim() + "'";
                SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();
                await DisplayAlert("Exito!", "La sesion se ha finalizado de manera remota", "OK");


                thisConnection.Open();
                cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                            "VALUES(GETDATE(),'" + usuariologueado + "','Sesion Cerrada Remotamente " + SesionCerrar.Trim() + "', 'CS')";
                cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                picker.SelectedIndex = -1;
            }
            else if (opcion == "Saltar Notificacion Producto")
            {
                string orden = await InputBoxOrden(this.Navigation, Indicador);


                string productoaomitir = await SelectedProducto(this.Navigation, orden, Indicador);

                string motivo = "";
                string prod_clave = "";

                string[] separadas;
                separadas = productoaomitir.Split('*');

                string[] snmotivo;
                snmotivo = separadas[0].Split('#');

                prod_clave = snmotivo[0];
                motivo = separadas[1];


                thisConnection.Open();
                string cadenainsert = "INSERT INTO   tb_det_sol_producto (fecha, imei, nom_usu, producto, observaciones, cantidad, ord_vent) " +
                           "VALUES(GETDATE(),'','" + usuariologueado + "','" + prod_clave + "','Intervension de Embarques ADM: " + motivo + "','0','" + orden.ToString().Trim() + "')";
                SqlCommand cmd = new SqlCommand(cadenainsert, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                await DisplayAlert("Exito!", "Producto enviado a Omision Exitosamente", "OK");

                thisConnection.Open();
                cadenainsert = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                            "VALUES(GETDATE(),'" + usuariologueado + "','Producto enviado a Omision de validacion " + prod_clave.Trim() + " Debido a " + motivo + "', 'OP')";
                cmd = new SqlCommand(cadenainsert, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                picker.SelectedIndex = -1;
            }
            else if (opcion == "Liberar Fotos de Verificacion Trailer")
            {
                //SelectedConseSelectedTrailer
                string SesionCerrar = await SelectedConse(this.Navigation);

            }
            else if (opcion == "Cambiar Estatus Surtido de Orden")
            {


                string permiso = "0";
                if (usuariologueado != "DANIEL ANTONIO MUÑOZ HERNANDEZ" && usuariologueado != "RICARDO CORTES")
                {
                    thisConnection.Open();
                    string cadenav = "SELECT CASE When (SELECT DATENAME(dw, GETDATE())) = 'Domingo' THEN '1' WHEN (SELECT DATENAME(dw, GETDATE())) = 'Sábado' AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'13:30:00', 108) HoraServidor)) THEN '2' WHEN ((SELECT DATENAME(dw, GETDATE())) NOT IN ('Domingo', 'Sábado') AND (SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'17:30:00', 108) HoraServidor)) THEN '2' ELSE '1' END";
                    SqlCommand cmdv = new SqlCommand(cadenav, thisConnection);
                    permiso = Convert.ToString(cmdv.ExecuteScalar());
                    thisConnection.Close();
                }

                if (permiso == "2")
                {
                    await DisplayAlert("Operacion Denegada!", "La operacion actual ha sido denegada debido a que esta activo el area de sistemas para realizar dicho movimiento, Favor de Notificar a Vigilancia el Levantamiento del Ticket", "OK");

                }
                else
                {
                    string ordendesvincular = await InputBox(this.Navigation);
                    string Tipoped = "NAL";
                    string tb_tabla = "tb_mstr_pedidos_nal";


                    if (ordendesvincular.Length > 0)
                    {
                        if (Convert.ToInt32(ordendesvincular) < 300000)
                        {
                            Tipoped = "EXP";
                            tb_tabla = "tb_mstr_pedidos_exp";

                        }
                    }

                    thisConnection.Open();
                    DateTime FechaHoy = DateTime.Now;

                    string Cadena = "Select pdn_fecha from " + tb_tabla + " Where pdn_folio = '" + ordendesvincular + "' AND pdn_estatus != 'C'";
                    SqlCommand cmdxi = new SqlCommand(Cadena, thisConnection);
                    DateTime FechaPedido = Convert.ToDateTime(cmdxi.ExecuteScalar());

                    TimeSpan tspan = FechaHoy - FechaPedido;

                    int dias = tspan.Days;

                    thisConnection.Close();
                    if (dias > 15)
                    {
                        await DisplayAlert("Error!", "El pedido es Mayor a 15 Dias", "OK");
                    }
                    else
                    {
                        if (Tipoped == "EXP")
                        {
                            ordendesvincular = "0" + ordendesvincular;
                        }

                        thisConnection.Open();

                        Cadena = "Select emb_folio from tb_mstr_embarque Where emb_folio = '" + ordendesvincular + "'";
                        cmdxi = new SqlCommand(Cadena, thisConnection);
                        string emb_folio = Convert.ToString(cmdxi.ExecuteScalar());
                        thisConnection.Close();

                        if (emb_folio.Length > 0)
                        {
                            await DisplayAlert("Error!", "El pedido Cuenta con Embarque, Favor de Cancelarlo", "OK");
                        }
                        else
                        {
                            thisConnection.Open();
                            string cadena = "UPDATE " + tb_tabla + " SET pdn_surtido = '' Where pdn_folio = '" + Convert.ToInt32(ordendesvincular) + "' AND pdn_estatus != 'C'";
                            SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                            cmd.ExecuteNonQuery();
                            thisConnection.Close();

                            await DisplayAlert("Exito!", "La orden a cambiado a No Surtido", "OK");

                            thisConnection.Open();
                            cadena = "INSERT INTO tb_det_acceso_EmbarquesADM (FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                                        "VALUES(GETDATE(),'" + usuariologueado + "','Orden Cambiada a No Surtida" + Convert.ToInt32(ordendesvincular) + "', 'DS')";
                            cmd = new SqlCommand(cadena, thisConnection);
                            cmd.ExecuteNonQuery();
                            thisConnection.Close();
                        }

                    }
                }
                picker.SelectedIndex = -1;
            }
            else if (opcion == "Autorizar Nuevas Ordenes")
            {

                string ordenAutorizar = await SelectedOrdenes(this.Navigation);

                string Tipoped = "NAL";
                string tb_tabla = "tb_mstr_pedidos_nal";


                if (ordenAutorizar.Length > 0)
                {
                    if (Convert.ToInt32(ordenAutorizar) < 300000)
                    {
                        Tipoped = "EXP";
                        tb_tabla = "tb_mstr_pedidos_exp";

                    }
                }

                thisConnection.Open();
                string cadena = "UPDATE " + tb_tabla + " SET pdn_autorizado = '' Where pdn_folio = '" + Convert.ToInt32(ordenAutorizar) + "' AND pdn_estatus != 'C'";
                SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                string vendedor = "";
                thisConnection.Open();
                cadena = "SELECT usu_email FROM " + tb_tabla + " JOIN tb_cat_usuarios ON usu_login = pdn_elaboro WHERE (pdn_folio = '" + Convert.ToInt32(ordenAutorizar.ToString().Trim()) + "')";
                cmd = new SqlCommand(cadena, thisConnection);
                reader1 = cmd.ExecuteReader();
                while (reader1.Read())
                {
                    vendedor = reader1.GetString(0).ToString().Trim();
                }
                thisConnection.Close();

                await DisplayAlert("Exito!", "Se autorizo la orden " + ordenAutorizar + " para Realizar una modificación", "OK");

                SendMail("dmunoz@mrlucky.com.mx; ricardo.cortes@mrlucky.com.mx; ahernandez@mrlucky.com.mx; logistica@mrlucky.com.mx; mdelrio@mrlucky.com.mx; " + vendedor, "Buen dia, La Orden " + ordenAutorizar + " ha sido Autorizada Para Carga", "Orden Autorizada Para Carga " + ordenAutorizar);

                thisConnection.Open();
                cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                            "VALUES(GETDATE(),'" + usuariologueado + "','Orden Autorizada Remotamente " + ordenAutorizar.Trim() + "', 'OA')";
                cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                picker.SelectedIndex = -1;

            }
            else if (opcion == "Autorizar Cambios en Ordenes")
            {
                string ordenAutorizar = await InputBox(this.Navigation);
                string Tipoped = "NAL";
                string tb_tabla = "tb_mstr_pedidos_nal";


                if (ordenAutorizar.Length > 0)
                {
                    if (Convert.ToInt32(ordenAutorizar) < 300000)
                    {
                        Tipoped = "EXP";
                        tb_tabla = "tb_mstr_pedidos_exp";

                    }
                }

                thisConnection.Open();
                string cadena = "UPDATE " + tb_tabla + " SET pdn_autorizado = 'A' Where pdn_folio = '" + Convert.ToInt32(ordenAutorizar) + "' AND pdn_estatus != 'C'";
                SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                string vendedor = "";
                thisConnection.Open();
                cadena = "SELECT usu_email FROM " + tb_tabla + " JOIN tb_cat_usuarios ON usu_login = pdn_elaboro WHERE (pdn_folio = '" + Convert.ToInt32(ordenAutorizar.ToString().Trim()) + "')";
                cmd = new SqlCommand(cadena, thisConnection);
                reader1 = cmd.ExecuteReader();
                while (reader1.Read())
                {
                    vendedor = reader1.GetString(0).ToString().Trim();
                }
                thisConnection.Close();



                await DisplayAlert("Exito!", "Se autorizo la orden " + ordenAutorizar + " para Realizar una modificación", "OK");

                SendMail("dmunoz@mrlucky.com.mx; ricardo.cortes@mrlucky.com.mx; ahernandez@mrlucky.com.mx; logistica@mrlucky.com.mx; mdelrio@mrlucky.com.mx; " + vendedor, "Buen dia, La Orden " + ordenAutorizar + " ha sido Autorizada Para Modificaciones", "Orden Autorizada Para Modificacion " + ordenAutorizar);

            }
            else if (opcion == "Sacar Trailer del Sistema")
            {
                string permiso = "0";
                if (usuariologueado != "DANIEL ANTONIO MUÑOZ HERNANDEZ" && usuariologueado != "RICARDO CORTES")
                {
                    thisConnection.Open();
                    string cadenav = "SELECT CASE When (SELECT DATENAME(dw, GETDATE())) = 'Domingo' THEN '1' WHEN (SELECT DATENAME(dw, GETDATE())) = 'Sábado' AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'13:30:00', 108) HoraServidor)) THEN '2' WHEN ((SELECT DATENAME(dw, GETDATE())) NOT IN ('Domingo', 'Sábado') AND (SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) > (SELECT Convert(datetime,'08:30:00', 108) HoraServidor)) AND ((SELECT CONVERT(datetime, (SELECT Convert(varchar(8),GetDate(), 108) hora), 108)) < (SELECT Convert(datetime,'17:30:00', 108) HoraServidor)) THEN '2' ELSE '1' END";
                    SqlCommand cmdv = new SqlCommand(cadenav, thisConnection);
                    permiso = Convert.ToString(cmdv.ExecuteScalar());
                    thisConnection.Close();
                }

                if (permiso == "2")
                {
                    await DisplayAlert("Operacion Denegada!", "La operacion actual ha sido denegada debido a que esta activo el area de sistemas para realizar dicho movimiento, Favor de Notificar a Vigilancia el Levantamiento del Ticket", "OK");

                }
                else
                {
                    string productoaomitir = await SelectedTrailer(this.Navigation);

                    string conse = "";
                    string motivo = "";
                    string no_trailer = "";
                    string horatrailer = "";

                    string[] separadas;
                    separadas = productoaomitir.Split('*');

                    string[] snmotivo;
                    snmotivo = separadas[0].Split('-');

                    conse = snmotivo[0].Trim();
                    no_trailer = snmotivo[1].Trim();
                    horatrailer = snmotivo[2].Trim();
                    motivo = separadas[1];


                    thisConnection.Open();

                    string Cadena = "Select count(emb_folio) from tb_mstr_embarque Where no_trailer = '" + no_trailer + "' AND hora_trailer = '" + horatrailer + "'";
                    SqlCommand cmdxi = new SqlCommand(Cadena, thisConnection);
                    string emb_folio = Convert.ToString(cmdxi.ExecuteScalar());
                    thisConnection.Close();

                    if (emb_folio != "0")
                    {
                        await DisplayAlert("Error!", "El Trailer Cuenta con Embarque, Favor de Cancelar los embarques para poder sacarlo del sistema", "OK");
                    }
                    else
                    {
                        thisConnection.Open();
                        string cadena = "DELETE from tb_mstr_trailer Where no_trailer = '" + no_trailer + "' AND hora_trailer = '" + horatrailer + "'";
                        SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                        cmd.ExecuteNonQuery();
                        thisConnection.Close();

                        await DisplayAlert("Exito!", "Trailer Eliminado del sistema Correctamente", "OK");

                        thisConnection.Open();
                        string cadenainsert = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                                    "VALUES(GETDATE(),'" + usuariologueado + "','Trailer Sacado del sistema " + no_trailer.Trim() + " Del dia " + horatrailer + " Debido a " + motivo + "', 'FS')";
                        cmd = new SqlCommand(cadenainsert, thisConnection);
                        cmd.ExecuteNonQuery();
                        thisConnection.Close();
                    }

                    picker.SelectedIndex = -1;
                }
            }
            else if (opcion == "Desactivar Activar Funciones Presplit")
            {
                if (usuariologueado != "DANIEL ANTONIO MUÑOZ HERNANDEZ" && usuariologueado != "RICARDO CORTES")
                {
                    string ordenAutorizar = await SelectedActivarDesactivarMejorasPresplit(this.Navigation);
                    string stsreetiquetado = "0";

                    if (ordenAutorizar == "ACTIVADO")
                    {
                        stsreetiquetado = "1";
                    }

                    thisConnection.Open();
                    string cadena = "UPDATE Tb_Reetiquetadohabilitar SET sts_reetiquetado = '" + stsreetiquetado + "'";
                    SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    await DisplayAlert("Exito!", "Se modifico el estatus de Reetiquetado y Habilitado a " + ordenAutorizar + " ", "OK");


                    picker.SelectedIndex = -1;
                }
                else {
                    await DisplayAlert("ERROR!", "Este Usuario no tiene acceso a este modulo", "OK");
                    picker.SelectedIndex = -1;
                }
            }
            else if (opcion == "Autorizar Ajuste Baja Por Producto")
            {
                string orden = await IngresarOrdenModificar(this.Navigation, Indicador);


                string productoaomitir = await SeleccionarProducto(this.Navigation, orden, Indicador);

                string motivo = "";
                string prod_clave = "";

                string[] separadas;
                separadas = productoaomitir.Split('*');

                string[] snmotivo;
                snmotivo = separadas[0].Split('#');

                prod_clave = snmotivo[0];
                motivo = separadas[1];


                thisConnection.Open();
                string cadenainsert = "UPDATE tb_det_pedidos SET pdn_modif = '1' WHERE prod_clave = '" + prod_clave + "' AND pdn_tipo = 'NAL'";
                SqlCommand cmd = new SqlCommand(cadenainsert, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                await DisplayAlert("Exito!", "Producto Autorizado para Modificacion", "OK");

                thisConnection.Open();
                cadenainsert = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                            "VALUES(GETDATE(),'" + usuariologueado + "','Producto enviado a Autorizacion de Cambio: " + prod_clave.Trim() + " Debido a " + motivo + "', 'PC')";
                cmd = new SqlCommand(cadenainsert, thisConnection);
                cmd.ExecuteNonQuery();
                thisConnection.Close();

                picker.SelectedIndex = -1;

                string vendedor = "";
                thisConnection.Open();
                string cadena = "SELECT usu_email FROM tb_mstr_pedidos_nal JOIN tb_cat_usuarios ON usu_login = pdn_elaboro WHERE (pdn_folio = '" + Convert.ToInt32(orden.ToString().Trim()) + "')";
                cmd = new SqlCommand(cadena, thisConnection);
                reader1 = cmd.ExecuteReader();
                while (reader1.Read())
                {
                    vendedor = reader1.GetString(0).ToString().Trim();
                }
                thisConnection.Close();

                SendMail("dmunoz@mrlucky.com.mx; ricardo.cortes@mrlucky.com.mx; ahernandez@mrlucky.com.mx; logistica@mrlucky.com.mx; mdelrio@mrlucky.com.mx; " + vendedor, "Buen dia, La Orden " + orden + " ha sido Autorizada Para Modificaciones En el producto" + prod_clave, "Orden Autorizada Para Modificacion " + orden + " En Producto");

            }

        }


        public static Task<string> InputBox(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Orden de Venta", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese Orden de Venta:" };
            var txtInput = new Entry { Keyboard = Keyboard.Numeric, Text = "", MaxLength = 6 };

            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                
                if (txtInput.Text.Trim().Length > 0)
                {
                    var result = txtInput.Text;
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else {
                    txtInput.Focus();
                }

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public static Task<string> InputBoxOrden(INavigation navigation, ActivityIndicator Indicador)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Orden de Venta", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese Orden de Venta:" };
            var txtInput = new Entry { Keyboard = Keyboard.Numeric, Text = "", MaxLength = 6 };

            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page

                if (txtInput.Text.Trim().Length > 0)
                {
                    Indicador.IsRunning = true;
                    Indicador.IsVisible = true;
                    var result = txtInput.Text;
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    txtInput.Focus();
                }

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public static Task<string> IngresarOrdenModificar(INavigation navigation, ActivityIndicator Indicador)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Orden de Venta", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese Orden de Venta:" };
            var txtInput = new Entry { Keyboard = Keyboard.Numeric, Text = "", MaxLength = 6 };

            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page

                if (txtInput.Text.Trim().Length > 0)
                {
                    Indicador.IsRunning = true;
                    Indicador.IsVisible = true;
                    var result = txtInput.Text;
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    txtInput.Focus();
                }

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public static Task<string> SelectedBox(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Seleccion de Responsable", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese el Usuario a Cerrar:" };
            var selectInput = new Picker {
                Title = "Reponsable de La Sesion",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };


            SqlConnection thisConnection = new SqlConnection(LoginPage.cadenaConexion);
            thisConnection.Open();
            string cadena = "Select * From  tb_det_acceso_celulares where folio = '' and estado = 'A'";
            SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "ConsPed");
            var ConsPed = ds.Tables["ConsPed"];
            foreach (DataRow Row in ConsPed.Rows)
            {
                selectInput.Items.Add(Row["nom_usu"].ToString());
            }
            thisConnection.Close();



            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex];
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    selectInput.Focus();
                }
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public static Task<string> SelectedOrdenes(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Seleccion de Orden", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Seleccione la Orden a Autorizar:" };
            var selectInput = new Picker
            {
                Title = "Ordenes en Estado Pendiente",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };


            SqlConnection thisConnection = new SqlConnection(LoginPage.cadenaConexion);
            thisConnection.Open();
            string cadena = "Select pdn_folio From  tb_mstr_pedidos_nal where pdn_autorizado = 'P' and pdn_estatus != 'C' UNION Select pdn_folio From  tb_mstr_pedidos_exp where pdn_autorizado = 'P' and pdn_estatus != 'C'";
            SqlDataAdapter da = new SqlDataAdapter(cadena, thisConnection);
            DataSet ds = new DataSet();
            da.Fill(ds, "ConsPed");
            var ConsPed = ds.Tables["ConsPed"];
            foreach (DataRow Row in ConsPed.Rows)
            {
                selectInput.Items.Add(Row["pdn_folio"].ToString());
            }
            thisConnection.Close();



            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex];
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    selectInput.Focus();
                }
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public static Task<string> SelectedActivarDesactivarMejorasPresplit(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Seleccion Activar/Desactivar", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Seleccione Opcion" };
            var selectInput = new Picker
            {
                Title = "Seleccione Opcion",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            selectInput.Items.Add("ACTIVADO");
            selectInput.Items.Add("DESACTVADO");


            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex];
                    await navigation.PopModalAsync();
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    selectInput.Focus();
                }
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }



        public Task<string> SelectedProducto(INavigation navigation, string folio, ActivityIndicator Indicador)
        {
            
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();
            string Tipopedido = "NAL";
            string Pedidoembarque = folio;


            var lblTitle = new Label { Text = "Seleccion del Producto", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese el Producto a omitir Validacion:" };
            var selectInput = new Picker
            {
                Title = "Producto a Omitir",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            var lblMessageotro = new Label { Text = "Ingrese el Motivo de la Omision:" };
            var txtInput = new Entry { Text = "" };


            if (Convert.ToInt32(folio) < 400000)
            {
                Tipopedido = "EXP";
                Pedidoembarque = "0" + Convert.ToInt32(folio);
            }

            
            thisConnection.Open();
            int faltanteporsurtir = 0;

            //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            string cadena = "Select A.prod_clave, B.prod_nombre, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A JOIN tb_cat_producto B ON A.prod_clave = B.prod_clave WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            SqlDataAdapter adapterInfo = new SqlDataAdapter(cadena, thisConnection);
            DataSet setinfo = new DataSet();
            adapterInfo.Fill(setinfo, "ConsPed");
            var informacion = setinfo.Tables["ConsPed"];
            thisConnection.Close();
            int productoexistente = 0;
            int Existencia = 0;
            foreach (DataRow Row in informacion.Rows)
            {
                faltanteporsurtir = (Convert.ToInt32(Row["pdn_num_unidades"].ToString().Trim().Replace(".000", "")) - (Convert.ToInt32(Row["surtido"].ToString().Trim())));
                if (faltanteporsurtir < 30 && faltanteporsurtir > 0)
                {
                    if (Convert.ToInt32(Row["solicitud"].ToString().Trim()) == 0)
                    {
                        Existencia = 0;
                        productoexistente = 0;
                        string claveproducto = Row["prod_clave"].ToString().Trim();
                        Genera(claveproducto);
                        foreach (DataRow fila in Inven.Rows)
                        {
                            Existencia = Existencia + 1;
                            if (Convert.ToInt32(fila["Dias"].ToString()) > 8 && (fila["Existencia"].ToString().Trim() != "" || fila["Existencia"].ToString().Trim() != "0"))
                            {
                                productoexistente = productoexistente + 1;
                            }
                        }
                        selectInput.Items.Add(Row["prod_clave"].ToString() + " # " + Row["prod_nombre"].ToString() + " E:" + Existencia + " EC:" + productoexistente);
                    }
                }
            }



            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex] + " * " + txtInput.Text;
                    await navigation.PopModalAsync();
                    Indicador.IsRunning = false;
                    Indicador.IsVisible = false;
                    // pass result
                    tcs.SetResult(result);
                }
                else if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length == 0)
                {
                    txtInput.Focus();
                }
                else {
                    selectInput.Focus();
                }
                // close page

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                Indicador.IsRunning = false;
                Indicador.IsVisible = false;
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, lblMessageotro, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        public Task<string> SeleccionarProducto(INavigation navigation, string folio, ActivityIndicator Indicador)
        {

            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();
            string Tipopedido = "NAL";
            string Pedidoembarque = folio;


            var lblTitle = new Label { Text = "Seleccion del Producto", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese el Producto a disminuir Existencia" };
            var selectInput = new Picker
            {
                Title = "Producto a Omitir",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            var lblMessageotro = new Label { Text = "Ingrese el Motivo de la modificacion:" };
            var txtInput = new Entry { Text = "" };


            if (Convert.ToInt32(folio) < 400000)
            {
                Tipopedido = "EXP";
                Pedidoembarque = "0" + Convert.ToInt32(folio);
            }


            thisConnection.Open();
            int faltanteporsurtir = 0;

            //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            string cadena = "Select A.prod_clave, B.prod_nombre, A.pdn_num_unidades FROM tb_det_pedidos A JOIN tb_cat_producto B ON A.prod_clave = B.prod_clave WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            SqlDataAdapter adapterInfo = new SqlDataAdapter(cadena, thisConnection);
            DataSet setinfo = new DataSet();
            adapterInfo.Fill(setinfo, "ConsPed");
            var informacion = setinfo.Tables["ConsPed"];
            thisConnection.Close();
            int productoexistente = 0;
            int Existencia = 0;
            foreach (DataRow Row in informacion.Rows)
            {
                
                selectInput.Items.Add(Row["prod_clave"].ToString() + " # " + Row["prod_nombre"].ToString() + " E:" + Existencia + " EC:" + productoexistente);
                   
            }



            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex] + " * " + txtInput.Text;
                    await navigation.PopModalAsync();
                    Indicador.IsRunning = false;
                    Indicador.IsVisible = false;
                    // pass result
                    tcs.SetResult(result);
                }
                else if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length == 0)
                {
                    txtInput.Focus();
                }
                else
                {
                    selectInput.Focus();
                }
                // close page

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                Indicador.IsRunning = false;
                Indicador.IsVisible = false;
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, lblMessageotro, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }



        public Task<string> SelectedConse(INavigation navigation)
        {

            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();
            string Tipopedido = "NAL";
            int porcentaje = 0;
            string consecutivo = "";


            var lblTitle = new Label { Text = "Eliminacion de Foto sistema Verificacion de Trailer", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese el Consecutivo:" };
            var selectInput = new Picker
            {
                Title = "Consecutivo - Placa a Eliminar Foto",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            var lblMessageotro = new Label { Text = "Seleccione la Foto a Eliminar:" };
            var selectInputfoto = new ListView
            {
                
            };

            thisConnection.Open();
            //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            string cadena = "SELECT * FROM tb_mstr_trailer A JOIN tb_det_revision_trailer B ON A.CONSE = B.CONSE AND A.FECHA = B.FECHA WHERE (A.fecha = (SELECT CONVERT(varchar, GETDATE(), 3) AS Expr1))";
            SqlDataAdapter adapterInfo = new SqlDataAdapter(cadena, thisConnection);
            DataSet setinfo = new DataSet();
            adapterInfo.Fill(setinfo, "ConsPed");
            var informacion = setinfo.Tables["ConsPed"];
            thisConnection.Close();
            int productoexistente = 0;
            int Existencia = 0;
            foreach (DataRow Row in informacion.Rows)
            {
                selectInput.Items.Add(Row["conse"].ToString() + " - " + Row["no_trailer"].ToString());
                Existencia = Existencia + 1;
            }

            if (Existencia == 0) {
                thisConnection.Open();
                //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
                cadena = "SELECT * FROM tb_mstr_trailer A JOIN tb_det_revision_trailer B ON A.CONSE = B.CONSE AND A.FECHA = B.FECHA WHERE (A.fecha = (SELECT CONVERT(varchar, (GETDATE() -1), 3) AS Expr1))";
                adapterInfo = new SqlDataAdapter(cadena, thisConnection);
                setinfo = new DataSet();
                adapterInfo.Fill(setinfo, "ConsPed");
                informacion = setinfo.Tables["ConsPed"];
                thisConnection.Close();
                foreach (DataRow Row in informacion.Rows)
                {
                    selectInput.Items.Add(Row["conse"].ToString() + " - " + Row["no_trailer"].ToString());
                }
            }

            selectInput.SelectedIndexChanged += async (s, e) => {
                porcentaje = 0;
                consecutivo = ((string)selectInput.Items[selectInput.SelectedIndex]);
                string[] array = consecutivo.Split('-');
                consecutivo = array[0];
                thisConnection.Open();
                //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
                string cadenax = "SELECT * FROM tb_det_revision_trailer WHERE (fecha = (SELECT CONVERT(varchar, GETDATE(), 3) AS Expr1)) AND conse = '"+ consecutivo + "'";
                SqlDataAdapter adapterInfox = new SqlDataAdapter(cadenax, thisConnection);
                DataSet setinfox = new DataSet();
                adapterInfox.Fill(setinfox, "ConsPed");
                var informacionx = setinfox.Tables["ConsPed"];
                thisConnection.Close();
                ObservableCollection<string> data = new ObservableCollection<string>();
                foreach (DataRow Row in informacionx.Rows)
                {
                    if (Row["setpointini"].ToString().Length > 0) {
                        data.Add("Set Point Inicial");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["numcaja"].ToString().Length > 0)
                    {
                        data.Add("Numero de Caja");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["difusor"].ToString().Length > 0)
                    {
                        data.Add("Difusor");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["piso"].ToString().Length > 0)
                    {
                        data.Add("Piso");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["cajacompleta"].ToString().Length > 0)
                    {
                        data.Add("Caja Completa");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["temprod1"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 1");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["temprod2"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 2");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["temprod3"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 3");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["temprod4"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 4");
                    }
                    if (Row["temprod5"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 5");
                    }
                    if (Row["temprod6"].ToString().Length > 0)
                    {
                        data.Add("Temperatura del Producto 6");
                    }
                    if (Row["setpointfin"].ToString().Length > 0)
                    {
                        data.Add("Set Point Final");
                        porcentaje = porcentaje + 10;
                    }
                    if (Row["vidrayan"].ToString().Length > 0)
                    {
                        data.Add("Video Rayan");
                    }
                    if (Row["termino_carga"].ToString().Length > 0)
                    {
                        data.Add("Termino de Carga");
                        porcentaje = porcentaje + 10;
                    }
                }
                selectInputfoto.ItemsSource = data;
                
            };

            selectInputfoto.ItemSelected += async (s, e) =>
            {
                string prueba = selectInputfoto.SelectedItem.ToString();
                string campo = "";


                if ("Set Point Inicial" == prueba)
                {
                    campo = "setpointini";
                    porcentaje = porcentaje - 10;
                }
                if ("Numero de Caja" == prueba)
                {
                    campo = "numcaja";
                    porcentaje = porcentaje - 10;
                }
                if ("Difusor" == prueba)
                {
                    campo = "difusor";
                    porcentaje = porcentaje - 10;
                }
                if ("Piso" == prueba)
                {
                    campo = "piso";
                    porcentaje = porcentaje - 10;
                }
                if ("Caja Completa" == prueba)
                {
                    campo = "cajacompleta";
                    porcentaje = porcentaje - 10;
                }
                if ("Temperatura del Producto 1" == prueba)
                {
                    campo = "temprod1";
                    porcentaje = porcentaje - 10;
                }
                if ("Temperatura del Producto 2" == prueba)
                {
                    campo = "temprod2";
                    porcentaje = porcentaje - 10;
                }

                if ("Temperatura del Producto 3" == prueba)
                {
                    campo = "temprod3";
                    porcentaje = porcentaje - 10;
                }
                if ("Temperatura del Producto 4" == prueba)
                {
                    campo = "temprod4";
                }
                if ("Temperatura del Producto 5" == prueba)
                {
                    campo = "temprod5";
                }
                if ("Temperatura del Producto 6" == prueba)
                {
                    campo = "temprod6";
                }
                if ("Set Point Final" == prueba)
                {
                    campo = "setpointfin";
                    porcentaje = porcentaje - 10;
                }
                if ("Video Rayan" == prueba)
                {
                    campo = "vidrayan";
                }
                if ("Termino de Carga" == prueba)
                {
                    campo = "termino_carga";
                    porcentaje = porcentaje - 10;
                }


                var action = await DisplayAlert("Borrar Foto", "¿Esta usted seguro que Desea Borrar la foto Actual?", "Si", "No");
                if (action)
                {
                    thisConnection.Open();
                    cadena = "UPDATE  tb_det_revision_trailer SET " + campo + " = null, porcentaje = '" + porcentaje + ".00' WHERE " +
                                "CONSE  = '" + consecutivo + "' and fecha = (SELECT CONVERT(varchar, GETDATE(), 3) AS Expr1)";
                    if (Existencia == 0)
                    {
                        cadena = "UPDATE  tb_det_revision_trailer SET " + campo + " = null, porcentaje = '" + porcentaje + ".00' WHERE " +
                                "CONSE  = '" + consecutivo + "' and fecha = (SELECT CONVERT(varchar, (GETDATE() -1), 3) AS Expr1)";

                    }
                    

                    SqlCommand cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    thisConnection.Open();
                    cadena = "INSERT INTO tb_det_acceso_EmbarquesADM(FECHA,NOM_USU,DETALLE, Tipo_Mov) " +
                                "VALUES(GETDATE(),'" + usuariologueado + "','Foto Eliminada del consecutivo " + consecutivo.Trim() + " Foto: " + prueba.Trim() + "', 'FE')";
                    cmd = new SqlCommand(cadena, thisConnection);
                    cmd.ExecuteNonQuery();
                    thisConnection.Close();

                    await DisplayAlert("Foto Borrada", "La Foto Ha Sido Borrada con Exito", "Entendido!");
                }
            };



                var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex];
                    await navigation.PopModalAsync();
                    Indicador.IsRunning = false;
                    Indicador.IsVisible = false;
                    // pass result
                    tcs.SetResult(result);
                }
                else
                {
                    selectInput.Focus();
                }
                // close page

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                Indicador.IsRunning = false;
                Indicador.IsVisible = false;
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, lblMessageotro, selectInputfoto, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }



        public Task<string> SelectedTrailer(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();



            var lblTitle = new Label { Text = "Seleccion del Trailer", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Ingrese el Trailer a omitir Validacion:" };
            var selectInput = new Picker
            {
                Title = "Producto a Omitir",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            var lblMessageotro = new Label { Text = "Ingrese el Motivo de la Omision:" };
            var txtInput = new Entry { Text = "" };



            thisConnection.Open();
            //string cadena = "Select A.prod_clave, A.pdn_num_unidades, ((SELECT isNull(SUM(cajas), 0) FROM tb_det_embarque WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND OpCap = 'N' AND Estatus = 'A' AND emb_Tipo = '" + Tipopedido + "') +  (SELECT isNull(SUM(cajas), 0) FROM tb_det_split WHERE emb_folio = '" + Pedidoembarque + "' AND prod_clave = A.prod_clave AND Estatus != 'C' AND emb_Tipo = '" + Tipopedido + "')) AS surtido, (SELECT COUNT(producto) from tb_det_sol_producto WHERE ord_vent = '" + Pedidoembarque + "' AND producto = A.prod_clave) AS solicitud FROM tb_det_pedidos A WHERE pdn_folio = '" + folio + "' AND pdn_Tipo = '" + Tipopedido + "'";
            string cadena = "SELECT * FROM Tb_mstr_trailer Where fecha <= (SELECT CONVERT(varchar, GETDATE(), 3) AS Expr1) and horafin = '--:--' AND guardar = 'N' ORDER BY fecha, conse ASC";
            SqlDataAdapter adapterInfo = new SqlDataAdapter(cadena, thisConnection);
            DataSet setinfo = new DataSet();
            adapterInfo.Fill(setinfo, "ConsPed");
            var informacion = setinfo.Tables["ConsPed"];
            thisConnection.Close();
            int productoexistente = 0;
            int Existencia = 0;
            foreach (DataRow Row in informacion.Rows)
            {
                selectInput.Items.Add(Row["conse"].ToString() + " - " + Row["no_trailer"].ToString() + " - " + Row["hora_trailer"].ToString());
                Existencia = Existencia + 1;
            }



            var btnOk = new Button
            {
                Text = "Aceptar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length > 0)
                {
                    var result = (string)selectInput.Items[selectInput.SelectedIndex] + " * " + txtInput.Text;
                    await navigation.PopModalAsync();
                    tcs.SetResult(result);
                }
                else if (((string)selectInput.Items[selectInput.SelectedIndex]).Length > 0 && txtInput.Text.Length == 0)
                {
                    txtInput.Focus();
                }
                else
                {
                    selectInput.Focus();
                }
                // close page

            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, selectInput, lblMessageotro, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            selectInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
            

        }



        private void CreaTable()
        {
            Inven.Columns.Add("Nombre", typeof(string));
            Inven.Columns.Add("FechaEla", typeof(string)); //int
            Inven.Columns.Add("Lote", typeof(string));
            Inven.Columns.Add("FecCad", typeof(string));
            Inven.Columns.Add("FecCadTeo", typeof(string));
            Inven.Columns.Add("Dias", typeof(int));
            Inven.Columns.Add("Existencia", typeof(int));
            Inven.Columns.Add("Cantidad", typeof(int));
            Inven.Columns.Add("Conse", typeof(int));
            Inven.Columns.Add("Prod", typeof(string));
            Inven.Columns.Add("CvePro", typeof(string));
            Inven.Columns.Add("Tipo", typeof(string));
            Inven.Columns.Add("FechaCad", typeof(string));
            Inven.Columns.Add("Ubica", typeof(string));
            Inven.Columns.Add("Tarima", typeof(string));
            Inven.Columns.Add("Presplit", typeof(int));

        }

        private void Genera(string claveprodcuto)
        {
            string x = System.DateTime.Now.ToString("dd-MM-yyyy");
            Inven.Rows.Clear();
            thisConnection.Open();
            DataSet ds = new DataSet();
            DataTable Info = new DataTable();
            string Cadena = "SELECT RECIBO,PROD_CLAVE, TARIMA, SUM(CAJAS) as CAJAS FROM tb_det_embarque " +
                            " WHERE SUBSTRING(FECHACAP,1,10) = '" + System.DateTime.Now.ToString("dd-MM-yyyy") + "' AND Estatus != 'C' " +
                            " AND ((CONVERT(INT,SUBSTRING(FECHACAP,12,2)) > 8 AND (CONVERT(INT,SUBSTRING(FECHACAP,12,2)) != 12) AND SUBSTRING(FECHACAP,21,1) = 'a') OR SUBSTRING(FECHACAP,21,1) = 'p') AND prod_clave = '" + claveprodcuto + "'" +
                            " GROUP BY PROD_CLAVE,RECIBO, TARIMA" +
                            " ORDER BY PROD_CLAVE,RECIBO, TARIMA";
            ds = new DataSet();
            Info = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "Surtido");
            DataTable Surtido = new DataTable();
            Surtido = ds.Tables["Surtido"];
            Cadena = "SELECT PROD_CLAVE, SUM(CAJAS) AS CAJAS FROM tb_det_split WHERE estatus = 'A' AND prod_clave = '" + claveprodcuto + "' GROUP BY prod_clave ORDER BY prod_clave";
            ds = new DataSet();
            Info = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "SPLIT");
            DataTable SPLIT = ds.Tables["SPLIT"];
            Cadena = "SELECT  PROD_CLAVE,SUM(ETIQUETA) AS CAJAS FROM TB_DET_TRAZABILIDAD WHERE PTI_FECHA =  '" + System.DateTime.Now.ToString("dd-MM-yyyy") + "' AND prod_clave = '" + claveprodcuto + "' AND tipo = 'PTC'" +
                     " GROUP BY PROD_CLAVE ORDER BY PROD_CLAVE ";
            ds = new DataSet();
            DataTable PTC = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "PTC");
            PTC = ds.Tables["PTC"];
            //string Cadena = "SELECT PROD_NOMBRE, RECIBO, TARIMA, PTI_FECHA, LOTE, FECHA_CAD, ETIQUETA, SURTIDO, PROD_CLAVE FROM TB_DET_TRAZABILIDAD WHERE PTI_ESTATUS_SUR =  ' ' AND TIPO = 'PTC'  ORDER BY PROD_NOMBRE,RECIBO,PTI_CLAVE ";
            Cadena = "SELECT C.PROD_NOMBRE, A.RECIBO, A.TARIMA, A.PTI_FECHA, A.LOTE, A.FECHA_CAD, A.ETIQUETA, A.SURTIDO, A.PROD_CLAVE, A.UBICACION " +
                            " FROM TB_DET_TRAZABILIDAD A, tb_mstr_recepcion_pt B, tb_cat_producto C " +
                            " WHERE A.PTI_ESTATUS_SUR =  ' ' AND A.prod_clave = '" + claveprodcuto + "' AND A.TIPO = 'PTC'  AND A.recibo = B.rpt_recibo AND A.PROD_CLAVE = C.PROD_CLAVE AND B.rpt_estatus = ' ' AND (B.rpt_tipo != 'TR' OR (B.rpt_tipo = 'TR' AND B.RPT_INVENTARIO = 'S'))" +
                            " ORDER BY PROD_NOMBRE,RECIBO,PTI_CLAVE ";
            ds = new DataSet();
            Info = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "Info");
            Info = ds.Tables["Info"];

            Cadena = "SELECT Eti_Recibo,Eti_Producto, Eti_TarIni, COUNT(Eti_Caja) as CAJAS FROM Tb_Det_Etiqueta_Presplit  WHERE Fecha = '" + System.DateTime.Now.ToString("dd-MM-yyyy") + "' AND Eti_Producto = '" + claveprodcuto + "' AND Estatus = 'A'  GROUP BY Eti_Producto,Eti_Recibo, Eti_TarIni ORDER BY Eti_Producto, Eti_Recibo, Eti_TarIni";
            ds = new DataSet();
            DataTable TempPresplit = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "TempPresplit");
            TempPresplit = ds.Tables["TempPresplit"];

            Cadena = "SELECT prod_clave, inv_teorico, inv_fisico " +
                     "FROM tb_mstr_inventario_fisico " +
                     "WHERE invpt_fecha = '" + System.DateTime.Now.ToString("dd-MM-yyyy") + "' AND prod_clave = '" + claveprodcuto + "' ORDER BY PROD_CLAVE ";
            ds = new DataSet();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "Teo");
            DataTable Teorico = new DataTable();
            Teorico = ds.Tables["Teo"];
            //dataGridView2.DataSource = Inven ;
            string Mnom = "", Nprod = "";
            int INI = 1, totp = 0, totg = 0, tott = 0;
            int prodPTC = 0;
            Int32 Teo = 0, Fisi = 0, Surti = 0;
            foreach (DataRow row in Info.Rows)
            {
                if (INI == 1)
                {
                    Mnom = row["PROD_NOMBRE"].ToString();
                    Nprod = row["prod_clave"].ToString();
                    //Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, Mnom, Nprod, ""); se bloqueo el 8 de abril 2017 original
                    Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, "", "", "");
                    INI = 0;
                }

                prodPTC = 1;
                //if (Mnom == "TOMATE ORGANICO MR.LUCKY CHERRY 12/10 OZ          ")
                //{ 
                //}

                string fol = row["RECIBO"].ToString();
                if (Mnom != row["PROD_NOMBRE"].ToString())
                {
                    Teo = 0; Fisi = 0; Surti = 0;
                    foreach (DataRow Row in Teorico.Select("PROD_CLAVE = '" + Nprod + "'"))
                    {
                        Teo = Convert.ToInt32(Row["INV_TEORICO"]);
                        Fisi = Convert.ToInt32(Row["INV_FISICO"]);
                    }
                    foreach (DataRow Row in Surtido.Select("prod_clave = '" + Nprod + "'"))
                    {
                        Surti = Surti + Convert.ToInt32(Row["Cajas"]);
                    }
                    foreach (DataRow Row in SPLIT.Select("prod_clave = '" + Nprod + "'"))
                    {
                        Surti = Surti + Convert.ToInt32(Row["cajas"]);
                    }
                    foreach (DataRow Row in PTC.Select("Prod_CLAVE = '" + Nprod + "'"))
                    {
                        tott = Convert.ToInt32(Row["Cajas"]);
                    }
                    //Inven.Rows.Add("TOTAL " + Mnom, "", "Teorico:", (Teo + tott).ToString(), "", Fisi + tott, 0, totp, 3, Mnom, Nprod, "", "99991231"); se bloqueo el 8 de abril 2017 original
                    Inven.Rows.Add("TOTAL " + Mnom, "", "", "", "", (Teo + tott - Surti).ToString(), Fisi + tott - Surti, totp, 3, Mnom, Nprod, "", "99991231");
                    Nprod = row["prod_clave"].ToString();
                    Mnom = row["PROD_NOMBRE"].ToString();
                    //Inven.Rows.Add(Mnom, "", "", Mnom, "", 0, 0, 0, 1, Mnom, Nprod, ""); se bloqueo el 8 de abril 2017 original
                    Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, "", "", "");
                    totp = 0; tott = 0;
                }

                totp = totp + (Convert.ToInt32(row["ETIQUETA"]) - Convert.ToInt32(row["SURTIDO"]));
                totg = totg + (Convert.ToInt32(row["ETIQUETA"]) - Convert.ToInt32(row["SURTIDO"]));
                //if (Convert.ToDateTime(row["PTI_FECHA"]).ToString("dd-MM-yyyy") == System.DateTime.Now.ToString("dd-MM-yyyy"))
                //    tott = tott + (Convert.ToInt32(row["ETIQUETA"])); // - Convert.ToInt32(row["SURTIDO"]));
                TimeSpan Mdias = TimeSpan.Zero;
                DateTime FecCad = Convert.ToDateTime(row["PTI_FECHA"]);
                string Ubica = Convert.ToString(row["ubicacion"]);
                string fechacaduemulada = "";
                string fechacaduemulada2 = "";
                if (row["FECHA_CAD"].ToString().Trim().Length > 0)
                {
                    string[] Fechacaducidad = row["FECHA_CAD"].ToString().Trim().Split('/');
                    fechacaduemulada = Fechacaducidad[1] + "/" + Fechacaducidad[0] + "/" + Fechacaducidad[2];

                    fechacaduemulada2 = Fechacaducidad[0] + "/" + Fechacaducidad[1] + "/" + Fechacaducidad[2];

                    string PRUEBA = row["FECHA_CAD"].ToString().Trim();

                    try
                    {
                        Mdias = Convert.ToDateTime(row["FECHA_CAD"].ToString().Trim()) - System.DateTime.Now.AddDays(-1);
                        FecCad = Convert.ToDateTime(row["FECHA_CAD"]);
                    }
                    catch
                    {
                        Mdias = Convert.ToDateTime(fechacaduemulada.ToString().Trim()) - System.DateTime.Now.AddDays(-1);
                        FecCad = Convert.ToDateTime(fechacaduemulada);
                    }

                }
                else
                {
                    if (Mnom.Contains("BETABEL"))
                        FecCad = FecCad.AddDays(60);
                    else
                        if (Mnom.Contains("AJO"))
                        FecCad = FecCad.AddDays(180);
                    else
                            if (Mnom.Contains("ADEREZO") || Mnom.Contains("VINAGRETA") || Mnom.Contains("QUESO"))
                        FecCad = FecCad.AddDays(90);
                    else
                        FecCad = FecCad.AddDays(14);
                    Mdias = FecCad - System.DateTime.Now.AddDays(-1);
                }

                int SURTISPRESPLIT = 0;
                foreach (DataRow Row in TempPresplit.Select("Eti_Recibo = '" + row["RECIBO"].ToString().Trim() + "' AND Eti_Producto = '" + Nprod.Trim() + "' AND Eti_TarIni = '" + row["TARIMA"].ToString().Trim() + "' "))
                {
                    SURTISPRESPLIT = Convert.ToInt32(Row["CAJAS"]);
                }
                string MNewFec = "";

                try
                {
                    MNewFec = (row["FECHA_CAD"].ToString().Trim().Length > 0) ? Convert.ToDateTime(fechacaduemulada.Trim()).ToString("yyyyMMdd") : FecCad.ToString("yyyyMMdd");
                }
                catch
                {
                    MNewFec = (row["FECHA_CAD"].ToString().Trim().Length > 0) ? Convert.ToDateTime(fechacaduemulada2.Trim()).ToString("yyyyMMdd") : FecCad.ToString("yyyyMMdd");
                    
                }

                Inven.Rows.Add(row["RECIBO"].ToString() + "-" + row["TARIMA"].ToString().Trim(), Convert.ToDateTime(row["PTI_FECHA"].ToString()).ToString("dd/MM/yyyy"), row["LOTE"].ToString(), (row["FECHA_CAD"].ToString().Trim().Length > 0) ? row["FECHA_CAD"] : FecCad.ToString("dd-MM-yyyy"), "", Mdias.Days, row["ETIQUETA"], (Convert.ToInt32(row["ETIQUETA"]) - Convert.ToInt32(row["SURTIDO"])), 2, Mnom, row["PROD_CLAVE"], "PTC", MNewFec, Ubica, row["TARIMA"].ToString().Trim(), SURTISPRESPLIT);

            }
            Teo = 0; Fisi = 0; Surti = 0;
            foreach (DataRow Row in Teorico.Select("PROD_CLAVE = '" + Nprod + "'"))
            {
                Teo = Convert.ToInt32(Row["INV_TEORICO"]);
                Fisi = Convert.ToInt32(Row["INV_FISICO"]);
            }
            foreach (DataRow Row in Surtido.Select("prod_clave = '" + Nprod + "'"))
            {
                Surti = Surti + Convert.ToInt32(Row["Cajas"]);
            }
            foreach (DataRow Row in SPLIT.Select("prod_clave = '" + Nprod + "'"))
            {
                Surti = Surti + Convert.ToInt32(Row["cajas"]);
            }
            foreach (DataRow Row in PTC.Select("prod_clave = '" + Nprod + "'"))
            {
                tott = Convert.ToInt32(Row["Cajas"]);
            }
            //Inven.Rows.Add("TOTAL " + Mnom, "", "", "TOTAL " + Mnom, "", 0, 0, totp, 3, Mnom, Nprod, "", "99991231"); se bloqueo el 8 de abril 2017 original
            if (prodPTC > 0)
            {
                Inven.Rows.Add("TOTAL " + Mnom, "", "", "", "", (Teo + tott - Surti).ToString(), Fisi + tott - Surti, totp, 3, Mnom, Nprod, "", "99991231");
            }
            // RECIBOS DE PRODUCCION
            totp = 0;
            //totg = 0;
            tott = 0;
            INI = 1;//
            Cadena = "SELECT  B.CVE_PROD,SUM(B.NUM_CAJAS) AS CAJAS FROM TB_DET_ETI_FINAL B WHERE B.FECHA =  '" + System.DateTime.Now.ToString("dd-MM-yyyy") + "' AND B.cve_prod = '" + claveprodcuto + "'" +
                     " GROUP BY CVE_PROD ORDER BY CVE_PROD ";
            ds = new DataSet();
            DataTable PTP = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "PTP");
            PTP = ds.Tables["PTP"];
            Cadena = "SELECT A.PROD_NOMBRE, B.FOLIO, B.TARIMA, B.FECHA, B.NUM_LOTE, B.NUM_CAJAS, B.CAJAS_SUR, B.CVE_PROD, B.UBICACION, b.fechacad FROM TB_DET_ETI_FINAL B, TB_CAT_PRODUCTO A WHERE B.ESTATUS_SUR =  ' ' AND B.CVE_PROD = A.PROD_CLAVE AND B.ETIQUETA = 'S' AND B.cve_prod = '" + claveprodcuto + "' ORDER BY A.PROD_NOMBRE,B.FOLIO,B.TARIMA ";
            ds = new DataSet();
            DataTable Info2 = new DataTable();
            da = new SqlDataAdapter(Cadena, thisConnection);
            da.Fill(ds, "Info2");
            Info2 = ds.Tables["Info2"];
            //datos = cmd.ExecuteReader();
            //while (datos.Read())
            //{
            //    Var_LoteSem = datos["semana"].ToString() + "-" + OleFE.Value.ToString("ddd").ToUpper();
            //}
            //Var_LoteSem = (Var_LoteSem.Trim().Length > 0) ? Var_LoteSem.Substring(0, 5) : ""; 
            int valorPTP = 0;
            foreach (DataRow row in Info2.Rows)
            {
                valorPTP = 1;
                if (INI == 1)
                {
                    Mnom = row["PROD_NOMBRE"].ToString();
                    Nprod = row["cve_prod"].ToString();
                    Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, "", "");
                    INI = 0;
                }

                if (Mnom != row["PROD_NOMBRE"].ToString())
                {
                    Teo = 0; Fisi = 0; Surti = 0;
                    foreach (DataRow Row in Teorico.Select("PROD_CLAVE = '" + Nprod + "'"))
                    {
                        Teo = Convert.ToInt32(Row["INV_TEORICO"]);
                        Fisi = Convert.ToInt32(Row["INV_FISICO"]);
                    }
                    foreach (DataRow Row in Surtido.Select("prod_clave = '" + Nprod + "'"))
                    {
                        Surti = Surti + Convert.ToInt32(Row["Cajas"]);
                    }
                    foreach (DataRow Row in SPLIT.Select("prod_clave = '" + Nprod + "'"))
                    {
                        Surti = Surti + Convert.ToInt32(Row["cajas"]);
                    }
                    foreach (DataRow Row in PTP.Select("cve_prod = '" + Nprod + "'"))
                    {
                        tott = Convert.ToInt32(Row["Cajas"]);
                    }
                    //Inven.Rows.Add("TOTAL " + Mnom, "", "Teorico:", (Teo + tott).ToString(), "", Fisi + tott, 0, totp, 3, Mnom, Nprod, "", "99991231"); se bloqueo el 8 de abril 2017 original
                    //if (Nprod == "16001LES18")
                    //    MessageBox.Show("hOLA");
                    Inven.Rows.Add("TOTAL " + Mnom, "", "", "", "", (Teo + tott - Surti).ToString(), Fisi + tott - Surti, totp, 3, Mnom, Nprod, "", "99991231");
                    //Inven.Rows.Add("TOTAL " + Mnom, "", "Teorico:", (Teo).ToString(), "", Fisi, 0, totp, 3, Mnom, Nprod, "", "99991231");
                    Mnom = row["PROD_NOMBRE"].ToString();
                    Nprod = row["cve_prod"].ToString();
                    //Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, Mnom, Nprod);se bloqueo el 8 de abril 2017 original
                    Inven.Rows.Add(Mnom, "", "", "", "", 0, 0, 0, 1, "", "");
                    totp = 0; tott = 0;
                }
                totp = totp + (Convert.ToInt32(row["NUM_CAJAS"]) - Convert.ToInt32(row["CAJAS_SUR"]));
                totg = totg + (Convert.ToInt32(row["NUM_CAJAS"]) - Convert.ToInt32(row["CAJAS_SUR"]));
                //if (Convert.ToDateTime(row["FECHA"]).ToString("dd-MM-yyyy") == System.DateTime.Now.ToString("dd-MM-yyyy"))
                //   tott = tott + (Convert.ToInt32(row["NUM_CAJAS"])); // - Convert.ToInt32(row["CAJAS_SUR"]));
                TimeSpan Mdias = TimeSpan.Zero;
                DateTime FecCad = Convert.ToDateTime(row["FECHA"]);
                string Ubica = Convert.ToString(row["ubicacion"]);
                string Mlot = "", Mfeca = "";
                if (row["NUM_LOTE"].ToString().Trim().Length > 0)
                {
                    int Mtam = row["NUM_LOTE"].ToString().Trim().Length;
                    if (row["fechacad"].ToString().Trim().Length > 0)
                        Mfeca = row["fechacad"].ToString().Substring(4, 2) + "/" + row["fechacad"].ToString().Substring(6, 2) + "/" + row["fechacad"].ToString().Substring(0, 4);
                    else
                        Mfeca = ConviertetoFecha(row["NUM_LOTE"].ToString().Substring((Mtam == 12) ? 7 : 6, 5));

                    string Mfol = row["FOLIO"].ToString();
                    try
                    {
                        Mdias = Convert.ToDateTime(Mfeca) - System.DateTime.Now.AddDays(-1);
                    }
                    catch {
                        if (row["fechacad"].ToString().Trim().Length > 0)
                            Mfeca = row["fechacad"].ToString().Substring(6, 2) + "/" + row["fechacad"].ToString().Substring(4, 2)  + "/" + row["fechacad"].ToString().Substring(0, 4);
                        else
                            Mfeca = ConviertetoFecha(row["NUM_LOTE"].ToString().Substring((Mtam == 12) ? 7 : 6, 5));

                        Mdias = Convert.ToDateTime(Mfeca) - System.DateTime.Now.AddDays(-1);
                    }
                    
                }
                else
                {
                    if (Mnom.Contains("BETABEL"))
                        FecCad = FecCad.AddDays(60);
                    else
                        if (Mnom.Contains("AJO"))
                        FecCad = FecCad.AddDays(180);
                    else
                            if (Mnom.Contains("ADEREZO") || Mnom.Contains("VINAGRETA") || Mnom.Contains("QUESO"))
                        FecCad = FecCad.AddDays(90);
                    else
                        FecCad = FecCad.AddDays(14);
                    Mdias = FecCad - System.DateTime.Now.AddDays(-1);
                    Mfeca = FecCad.ToString("MM-dd-yyyy");
                }
                Mlot = Lote(row["Fecha"].ToString());  //row["NUM_LOTE"].ToString().Substring(0, 4);
                //    Mdias = Convert.ToDateTime(row["FECHA_CAD"]) - System.DateTime.Now;
                string MNewFec = Convert.ToDateTime(Mfeca).ToString("yyyyMMdd");
                int SURTISPRESPLIT = 0;
                foreach (DataRow Row in TempPresplit.Select("Eti_Recibo = '" + row["FOLIO"].ToString().Trim() + "' AND Eti_Producto = '" + Nprod.Trim() + "' AND Eti_TarIni = '" + row["TARIMA"].ToString().Trim() + "' "))
                {
                    SURTISPRESPLIT = Convert.ToInt32(Row["CAJAS"]);
                }




                Inven.Rows.Add(row["FOLIO"].ToString() + "-" + row["TARIMA"].ToString().Trim(), Convert.ToDateTime(row["FECHA"].ToString()).ToString("dd/MM/yyyy"), Mlot, Mfeca, "", Mdias.Days, row["NUM_CAJAS"], (Convert.ToInt32(row["NUM_CAJAS"]) - Convert.ToInt32(row["CAJAS_SUR"])), 2, Mnom, Nprod, "PTP", MNewFec, Ubica, row["TARIMA"].ToString().Trim(), SURTISPRESPLIT);
            }
            Teo = 0; Fisi = 0; Surti = 0;
            foreach (DataRow Row in Teorico.Select("PROD_CLAVE = '" + Nprod + "'"))
            {
                Teo = Convert.ToInt32(Row["INV_TEORICO"]);
                Fisi = Convert.ToInt32(Row["INV_FISICO"]);
            }
            foreach (DataRow Row in Surtido.Select("prod_clave = '" + Nprod + "'"))
            {
                Surti = Surti + Convert.ToInt32(Row["Cajas"]);
            }
            foreach (DataRow Row in SPLIT.Select("prod_clave = '" + Nprod + "'"))
            {
                Surti = Surti + Convert.ToInt32(Row["cajas"]);
            }
            foreach (DataRow Row in PTP.Select("cve_prod = '" + Nprod + "'"))
            {
                tott = Convert.ToInt32(Row["Cajas"]);
            }

            if (valorPTP > 0)
            {
                Inven.Rows.Add("TOTAL " + Mnom, "", "", "", "", (Teo + tott - Surti).ToString(), Fisi + tott - Surti, totp, 3, Mnom, Nprod, "", "99991231");
            }


            Inven.DefaultView.Sort = "Prod, Conse, FechaCad ASC";
            Inven = Inven.DefaultView.ToTable();

            thisConnection.Close();
        }

        private string Lote(string Fecha)
        {
            string Cad = "";
            foreach (DataRow Row in Semanas.Select("fecha1 <= '" + Fecha + "' AND fecha2 >= '" + Fecha + "'"))
            {
                Cad = Row["semana"].ToString() + "-" + Convert.ToDateTime(Fecha).ToString("ddd").ToUpper();
            }
            Cad = (Cad.Trim().Length > 0) ? Cad.Substring(0, 5) : "";
            return Cad;
        }

        private string ConviertetoFecha(string FEC)
        {
            string mdia = FEC.Substring(3, 2);
            string mmes = FEC.Substring(0, 3);
            string nmes = "";
            if (mmes == "ENE")
                nmes = "01";
            if (mmes == "FEB")
                nmes = "02";
            if (mmes == "MAR")
                nmes = "03";
            if (mmes == "ABR")
                nmes = "04";
            if (mmes == "MAY")
                nmes = "05";
            if (mmes == "JUN")
                nmes = "06";
            if (mmes == "JUL")
                nmes = "07";
            if (mmes == "AGO")
                nmes = "08";
            if (mmes == "SEP")
                nmes = "09";
            if (mmes == "OCT")
                nmes = "10";
            if (mmes == "NOV")
                nmes = "11";
            if (mmes == "DIC")
                nmes = "12";
            int MES = System.DateTime.Now.Month;
            int anio = System.DateTime.Now.Year + (MES == 12 && nmes == "01" ? 1 : 0);
            //if (Convert.ToInt32(nmes) < MES)
            //    anio++;
            string cad = mdia + "/" + nmes + "/" + anio.ToString();
            return cad;
        }

        public void SendMail(string Dest, string mBody, string mAsunto)
        {
            MailMessage msg = new MailMessage();
            MailMessage email = new MailMessage();

            string[] destinatarios = Dest.Split(';');
            foreach (string destinos in destinatarios)
            {
                email.To.Add(new MailAddress(destinos));
            }
            //email.To.Add(new MailAddress("gcamacho@mrlucky.com.mx"));

            email.From = new MailAddress("ahernandez@mrlucky.com.mx"); //
            email.Subject = mAsunto; //"Mensaje de Prueba";
            email.Body = mBody;  //"Información de la factura";
            email.IsBodyHtml = true;
            email.Priority = MailPriority.High;


            SmtpClient smtp = new SmtpClient();
            smtp.Host = "mail1.mrlucky.com.mx";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("ahernandez", "operaciones");

            try
            {
                smtp.Send(email);
                email.Dispose();
                DisplayAlert("Exito!", "Se Envio el Correo", "OK");
            }
            catch (System.Exception ex)
            {

                DisplayAlert("Error!", "No se envio el correo debido a :" + ex, "OK");
            }
        }




    }
}