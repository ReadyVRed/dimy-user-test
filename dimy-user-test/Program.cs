using System.Data;
using static dimy_user_test.Services.DbHelper;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();


app.MapPost("/order", (Order request) =>
{
    Response<object> response = new();
    string sql;
    Dictionary<string, string> parameters = new();
    parameters.Add("@customerId", request.CustomerId.ToString());
    sql = "select * from customer where id=@customerId";
    DataSet ds = ExecuteDataset(sql, parameters);
    if (!CheckDataset(ds))
    {
        response.Status = false;
        response.Keterangan = "Data customer tidak ditemukan";
        return Results.Ok(response);
    }
    parameters.Clear();
    parameters.Add("customerId", request.CustomerId.ToString());
    parameters.Add("customerAddressId", request.AddressId.ToString());
    sql = "insert into order " +
    "(customer_id, customer_address_id, date, status) " +
    "values " +
    "(@customerId, @customerAddressId, now(), 0);";
    sql += "set @orderId=LAST_INSERT_ID();";
    for (int i = 0; i < request.ProdukIds?.Count; i++)
    {
        sql += "insert into order_item (order_id, product_id, price) " +
        "values " +
        $"(@orderId, {request.ProdukIds[i]}, (select price from product where id={request.ProdukIds[i]}));";
    }
    for (int i = 0; i < request.Payment?.Count; i++)
    {
        sql += "insert into payment (order_id, payment_method_id, nominal, date) " +
        "values " +
        $"(@orderId, {request.Payment[i].PaymentMethodId}, {request.Payment[i].Nominal}, now());";
    }
    int affectedRow = ExecuteNonQuery(sql, parameters);
    if (affectedRow <= 0)
    {
        response.Status = false;
        response.Keterangan = "Gagal melakukan order";
        return Results.Ok(response);
    }
    response.Status = true;
    response.Keterangan = "Sukses melakukan order, segera lakukan pembayaran";
    return Results.Ok(response);
});

app.Run();

class Response<T>
{
    public bool Status { get; set; }
    public string? Keterangan { get; set; }
    public T? Data { get; set; }
}
class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public List<int>? ProdukIds { get; set; }
    public List<Payment>? Payment { get; set; }
}
class Payment
{
    public int PaymentMethodId { get; set; }
    public decimal Nominal { get; set; }
}