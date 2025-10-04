using AMLSurvey.Infrastructure.Helpers;
using AMLSurvey.Infrastructure.Repositories.Base;
using Dapper;
using Microsoft.AspNetCore.Http;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace AMLSurvey.Infrastructure.Helpers
{
    public class OracleParametersBuilder
    {
        private readonly DynamicParameters _parameters;

        public OracleParametersBuilder()
        {
            _parameters = new DynamicParameters();
        }

        // ✅ Core Add Method (مع Oracle Native Support)
        private OracleParametersBuilder AddParameter(
            string name,
            object value,
            OracleDbType oracleType,
            ParameterDirection direction = ParameterDirection.Input,
            int? size = null)
        {
            var oracleParam = new OracleParameter
            {
                ParameterName = name,
                OracleDbType = oracleType,
                Direction = direction,
                Value = value ?? DBNull.Value
            };

            if (size.HasValue)
                oracleParam.Size = size.Value;

            _parameters.Add(name, oracleParam.Value, oracleParam.DbType, oracleParam.Direction, oracleParam.Size);

            return this;
        }

        // --------------------- Input Parameters ---------------------

        public OracleParametersBuilder AddStringInput(string name, string value, int? size = null)
        {
            return AddParameter(name, value, OracleDbType.Varchar2, size: size);
        }

        public OracleParametersBuilder AddIntInput(string name, int? value)
        {
            return AddParameter(name, value, OracleDbType.Int32);
        }

        public OracleParametersBuilder AddLongInput(string name, long? value)
        {
            return AddParameter(name, value, OracleDbType.Int64);
        }

        public OracleParametersBuilder AddDateInput(string name, DateTime? value)
        {
            return AddParameter(name, value, OracleDbType.Date);
        }

        public OracleParametersBuilder AddDecimalInput(string name, decimal? value)
        {
            return AddParameter(name, value, OracleDbType.Decimal);
        }

        public OracleParametersBuilder AddBooleanInput(string name, bool value)
        {
            // Oracle مفيهوش Boolean، بنستخدم 1/0
            return AddParameter(name, value ? 1 : 0, OracleDbType.Int32);
        }

        // ✅ CLOB (مهم للـ Large Text)
        public OracleParametersBuilder AddClobInput(string name, string value)
        {
            return AddParameter(name, value, OracleDbType.Clob);
        }

        // ✅ BLOB (للـ Binary Data)
        public OracleParametersBuilder AddBlobInput(string name, byte[] value)
        {
            return AddParameter(name, value, OracleDbType.Blob);
        }

        // --------------------- Output Parameters ---------------------

        public OracleParametersBuilder AddStringOutput(string name, int size = 4000)
        {
            return AddParameter(name, null, OracleDbType.Varchar2, ParameterDirection.Output, size);
        }

        public OracleParametersBuilder AddIntOutput(string name)
        {
            return AddParameter(name, null, OracleDbType.Int32, ParameterDirection.Output);
        }

        public OracleParametersBuilder AddDecimalOutput(string name)
        {
            return AddParameter(name, null, OracleDbType.Decimal, ParameterDirection.Output);
        }

        public OracleParametersBuilder AddDateOutput(string name)
        {
            return AddParameter(name, null, OracleDbType.Date, ParameterDirection.Output);
        }

        // ✅ RefCursor (مهم جداً!)
        public OracleParametersBuilder AddRefCursor(string name)
        {
            return AddParameter(name, null, OracleDbType.RefCursor, ParameterDirection.Output);
        }

        public OracleParametersBuilder AddRefCursors(params string[] cursorNames)
        {
            foreach (var name in cursorNames)
                AddRefCursor(name);
            return this;
        }

        // --------------------- Helpers for Common Scenarios ---------------------

        public OracleParametersBuilder AddUserAuthInputs(string email, string password, bool lockoutOnFailure)
        {
            return this
                .AddStringInput("p_email", email)
                .AddStringInput("p_password", password)
                .AddBooleanInput("p_lockout_on_failure", lockoutOnFailure);
        }

        public OracleParametersBuilder AddAuthOutputs()
        {
            return this
                .AddIntOutput("p_is_success")
                .AddStringOutput("p_error_code", 50);
        }

        public OracleParametersBuilder AddUserCreateInputs(string email, string firstName, string lastName, string password)
        {
            return this
                .AddStringInput("p_email", email)
                .AddStringInput("p_first_name", firstName)
                .AddStringInput("p_last_name", lastName)
                .AddStringInput("p_password", password);
        }

        public OracleParametersBuilder AddUserCreateOutputs()
        {
            return this
                .AddStringOutput("p_user_id", 450)
                .AddIntOutput("p_is_success")
                .AddStringOutput("p_error_message", 500);
        }

        public OracleParametersBuilder AddRefreshTokenInputs(string userId, string token, DateTime expiresOn, DateTime createdOn, string createdByIp = null)
        {
            return this
                .AddStringInput("p_user_id", userId)
                .AddStringInput("p_token", token)
                .AddDateInput("p_expires_on", expiresOn)
                .AddDateInput("p_created_on", createdOn)
                .AddStringInput("p_created_by_ip", createdByIp);
        }

        // --------------------- Build & Get ---------------------

        public DynamicParameters Build() => _parameters;

        public T Get<T>(string name) => _parameters.Get<T>(name);
    }
}

/*    test
 public ResponseStatus GET_ORG_LICN_DETAILS(string P_COMP_CIVIL_ID, string P_REQ_TYPE, string P_PERM_ID)
{
    ResponseStatus response = new ResponseStatus();

    HttpContext context = HttpContext.Current;
    Login_User_Data USER_DATA = HttpContext.Current.Session["USER_DATA"] as Login_User_Data;
    var USERID = USER_DATA.USERID;
    DataSet ds = new DataSet();
    OpenDB();
    string cmdString = "";

    cmdString = "APP_RE.RE_ORG_PKG.GET_ORG_LICN_DETAILS";
    cmd = new OracleCommand(cmdString, con);
    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.Add("P_LICN_CIVIL_ID", OracleDbType.Varchar2).Value = P_COMP_CIVIL_ID;
    cmd.Parameters.Add("P_LICN_CENTRAL_NO", OracleDbType.Varchar2).Value = DBNull.Value;
    cmd.Parameters.Add("P_LICN_COMM_NO", OracleDbType.Varchar2).Value = DBNull.Value;
    cmd.Parameters.Add("P_LANG", OracleDbType.Int64).Value = 0;
    cmd.Parameters.Add("P_USER", OracleDbType.Varchar2).Value = USERID;

    cmd.Parameters.Add("P_OWNERS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("P_ACTIVITY", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("P_LIC_DETAILS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("P_LIC_ADDRESS", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("P_STATUS", OracleDbType.Int64).Direction = ParameterDirection.Output;
    cmd.Parameters.Add("P_STATUS_DESC", OracleDbType.Varchar2, 32767).Direction = ParameterDirection.Output;

    // create a data adapter to use with the data set
    OracleDataAdapter da = new OracleDataAdapter(cmd);
    da.TableMappings.Add("Table", "P_OWNERS");
    da.TableMappings.Add("Table1", "P_ACTIVITY");
    da.TableMappings.Add("Table2", "P_LIC_DETAILS");
    da.TableMappings.Add("Table3", "P_LIC_ADDRESS");

    da.Fill(ds);
    response.Status = cmd.Parameters["P_STATUS"].Value.ToString();
    response.StatusDesc = cmd.Parameters["P_STATUS_DESC"].Value.ToString();
    if (response.Status == "1")
    {
        List<GET_ORG_LICN_OWNERS> lst = new List<GET_ORG_LICN_OWNERS>();
        if (ds.Tables["P_OWNERS"].Rows.Count > 0)
        {
            foreach (DataRow row in ds.Tables["P_OWNERS"].Rows)
            {
                GET_ORG_LICN_OWNERS obj3 = new GET_ORG_LICN_OWNERS();
                obj3.CIVIL_ID = row["CIVIL_ID"].ToString();
                obj3.CIVIL_NAME = row["CIVIL_NAME"].ToString();
                obj3.OWNER_MEMBER_DESC_ID = row["OWNER_MEMBER_DESC_ID"].ToString();
                obj3.OWNER_MEMBER_DESC_NAME = row["OWNER_MEMBER_DESC_NAME"].ToString();
                obj3.CIVIL_TYPE_ID = row["CIVIL_TYPE_ID"].ToString();
                obj3.CIVIL_TYPE_NAME = row["CIVIL_TYPE_NAME"].ToString();
                obj3.CIVIL_NATION_ID = row["CIVIL_NATION_ID"].ToString();
                obj3.CIVIL_NATION_NAME = row["CIVIL_NATION_NAME"].ToString();

                lst.Add(obj3);

            }
        }
        HttpContext.Current.Session["P_OWNERS"] = lst;

        List<GET_ORG_LICN_ACTIVITY> LicenseActivities = new List<GET_ORG_LICN_ACTIVITY>();
        if (ds.Tables["P_ACTIVITY"].Rows.Count > 0)
        {
            foreach (DataRow row in ds.Tables["P_ACTIVITY"].Rows)
            {
                GET_ORG_LICN_ACTIVITY obj5 = new GET_ORG_LICN_ACTIVITY();
                obj5.LICN_ACTIVITY_ID = row["LICN_ACTIVITY_ID"].ToString();
                obj5.LICN_RECID = row["LICN_RECID"].ToString();
                obj5.ACTIVITY_RECID = row["ACTIVITY_RECID"].ToString();
                obj5.ACTIVITY_DESC = row["ACTIVITY_DESC"].ToString();
                obj5.ACTIVITY_CODE = row["ACTIVITY_CODE"].ToString();
                obj5.ACTIVITY_CODE_INTL = row["ACTIVITY_CODE_INTL"].ToString();
                obj5.ACTIVITY_DESC_INTL = row["ACTIVITY_DESC_INTL"].ToString();

                LicenseActivities.Add(obj5);

            }
        }
        HttpContext.Current.Session["P_ACTIVITY"] = LicenseActivities;

        List<GET_ORG_LICN_ADDRESS> LicenseAddresses = new List<GET_ORG_LICN_ADDRESS>();
        if (ds.Tables["P_LIC_ADDRESS"].Rows.Count > 0)
        {
            foreach (DataRow row in ds.Tables["P_LIC_ADDRESS"].Rows)
            {
                GET_ORG_LICN_ADDRESS obj7 = new GET_ORG_LICN_ADDRESS();
                obj7.LICN_ADDRESS_ID = row["LICN_ADDRESS_ID"].ToString();
                obj7.LICN_RECID = row["LICN_RECID"].ToString();
                obj7.ADDRESS_AUTO_NO = row["ADDRESS_AUTO_NO"].ToString();
                obj7.GOV_CODE = row["GOV_CODE"].ToString();
                obj7.GOV_DESC = row["GOV_DESC"].ToString();
                obj7.DISTRICT_CODE = row["DISTRICT_CODE"].ToString();
                obj7.DISTRICT_DESC = row["DISTRICT_DESC"].ToString();
                obj7.BLOCK = row["BLOCK"].ToString();
                obj7.PLOT_NO = row["PLOT_NO"].ToString();
                obj7.STREET = row["STREET"].ToString();
                obj7.UNIT_TYPE = row["UNIT_TYPE"].ToString();
                obj7.UNIT_TYPE_DESC = row["UNIT_TYPE_DESC"].ToString();
                obj7.FLOOR_NO = row["FLOOR_NO"].ToString();
                obj7.UNIT_NO = row["UNIT_NO"].ToString();
                obj7.WV_BLDG_NAME = row["WV_BLDG_NAME"].ToString();

                LicenseAddresses.Add(obj7);

            }
        }

        GET_ORG_LICN_DETAILS licenseDetails = new GET_ORG_LICN_DETAILS();
        if (ds.Tables["P_LIC_DETAILS"].Rows.Count > 0)
        {
            licenseDetails.LINC_TYPE_DESC = ds.Tables["P_LIC_DETAILS"].Rows[0]["LINC_TYPE_DESC"].ToString();
            licenseDetails.LICN_TYP_RECID = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_TYP_RECID"].ToString();
            licenseDetails.LICN_REQ_RECID = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_REQ_RECID"].ToString();
            licenseDetails.COMPANY_NAME = ds.Tables["P_LIC_DETAILS"].Rows[0]["COMPANY_NAME"].ToString();
            licenseDetails.TRADE_NAME = ds.Tables["P_LIC_DETAILS"].Rows[0]["TRADE_NAME"].ToString();
            licenseDetails.LICN_CENTRAL_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_CENTRAL_NO"].ToString();
            licenseDetails.LICN_COMM_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_COMM_NO"].ToString();
            licenseDetails.COMM_BOOK_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["COMM_BOOK_NO"].ToString();
            licenseDetails.LICN_CIVIL_ID = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_CIVIL_ID"].ToString();
            licenseDetails.LICN_COMM_SDATE = GetDateFromNullableString(ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_COMM_SDATE"].ToString())?.ToString("dd/MM/yyyy") ?? string.Empty;
            licenseDetails.LICN_COMM_EDATE = GetDateFromNullableString(ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_COMM_EDATE"].ToString())?.ToString("dd/MM/yyyy") ?? string.Empty;
            licenseDetails.ADDRESS_AUTO_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["ADDRESS_AUTO_NO"].ToString();
            licenseDetails.LICN_STATUS = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_STATUS"].ToString();
            licenseDetails.LICN_STATUS_DESC = ds.Tables["P_LIC_DETAILS"].Rows[0]["LICN_STATUS_DESC"].ToString();
            licenseDetails.GOV_CODE = ds.Tables["P_LIC_DETAILS"].Rows[0]["GOV_CODE"].ToString();
            licenseDetails.GOV_DESC = ds.Tables["P_LIC_DETAILS"].Rows[0]["GOV_DESC"].ToString();
            licenseDetails.DISTRICT_CODE = ds.Tables["P_LIC_DETAILS"].Rows[0]["DISTRICT_CODE"].ToString();
            licenseDetails.DISTRICT_DESC = ds.Tables["P_LIC_DETAILS"].Rows[0]["DISTRICT_DESC"].ToString();
            licenseDetails.BLOCK = ds.Tables["P_LIC_DETAILS"].Rows[0]["BLOCK"].ToString();
            licenseDetails.PLOT_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["PLOT_NO"].ToString();
            licenseDetails.STREET = ds.Tables["P_LIC_DETAILS"].Rows[0]["STREET"].ToString();
            licenseDetails.UNIT_TYPE = ds.Tables["P_LIC_DETAILS"].Rows[0]["UNIT_TYPE"].ToString();
            licenseDetails.UNIT_TYPE_DESC = ds.Tables["P_LIC_DETAILS"].Rows[0]["UNIT_TYPE_DESC"].ToString();
            licenseDetails.FLOOR_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["FLOOR_NO"].ToString();
            licenseDetails.UNIT_NO = ds.Tables["P_LIC_DETAILS"].Rows[0]["UNIT_NO"].ToString();
            licenseDetails.WV_BLDG_NAME = ds.Tables["P_LIC_DETAILS"].Rows[0]["WV_BLDG_NAME"].ToString();
        }

        response.GET_ORG_LICN_OWNERS = lst;
        context.Session["P_OWNERS"] = null;
        context.Session["P_OWNERS"] = lst;
        response.GET_ORG_LICN_ACTIVITY = LicenseActivities;
        context.Session["P_ACTIVITY"] = null;
        context.Session["P_ACTIVITY"] = LicenseActivities;
        response.GET_ORG_LICN_ADDRESS = LicenseAddresses;
        response.GET_ORG_LICN_DETAILS = licenseDetails;

    }
    else
    {
        response.Status = "0";
        response.StatusDesc = cmd.Parameters["P_STATUS_DESC"].Value.ToString();
    }

    CloseDB();
    return response;
}
*/

/*  solution
 public class OracleLicenseRepository : BaseOracleRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OracleLicenseRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        : base(configuration)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseStatus> GetOrgLicnDetailsAsync(string civilId, string reqType, string permId, string userId, CancellationToken cancellationToken = default)
    {
        var parameters = OracleParametersBuilder.CreateParametersBuilder()
            .AddStringInput("P_LICN_CIVIL_ID", civilId)
            .AddStringInput("P_LICN_CENTRAL_NO", null)
            .AddStringInput("P_LICN_COMM_NO", null)
            .AddIntInput("P_LANG", 0)
            .AddStringInput("P_USER", userId)
            .AddRefCursor("P_OWNERS")
            .AddRefCursor("P_ACTIVITY")
            .AddRefCursor("P_LIC_DETAILS")
            .AddRefCursor("P_LIC_ADDRESS")
            .AddIntOutput("P_STATUS")
            .AddStringOutput("P_STATUS_DESC", 32767)
            .Build();

        using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var multi = await connection.QueryMultipleAsync(
            "APP_RE.RE_ORG_PKG.GET_ORG_LICN_DETAILS",
            param: parameters,
            commandType: CommandType.StoredProcedure
        );

        // قراءة RefCursors مباشرة
        var owners = (await multi.ReadAsync<OwnerDto>()).ToList();
          var activities = (await multi.ReadAsync<ActivityDto>()).ToList();
          var details = (await multi.ReadAsync<LicenseDetailsDto>()).FirstOrDefault();
          var addresses = (await multi.ReadAsync<AddressDto>()).ToList();

        // قراءة Output Parameters
        var status = ((DynamicParameters)parameters).Get<int>("P_STATUS");
        var statusDesc = ((DynamicParameters)parameters).Get<string>("P_STATUS_DESC");

        var response = new ResponseStatus
        {
            Status = status.ToString(),
            StatusDesc = statusDesc
        };

        if (status == 1)
        {
            var response = new LicnDetailsResponse  // تعريف كلاس جديد لتجميع البيانات
               {
                   Status = status.ToString(),
                   StatusDesc = statusDesc,
                   Owners = owners,
                   Activities = activities,
                   LicenseDetails = details,
                   Addresses = addresses
               };


            // حفظ البيانات في Session
           var session = _httpContextAccessor.HttpContext!.Session;
               session.Set("P_OWNERS", owners);
               session.Set("P_ACTIVITY", activities);
               session.Set("P_LIC_DETAILS", details);
               session.Set("P_LIC_ADDRESS", addresses);
              

        return response;
    }
}
}*/

/* best solution
 public class OracleLicenseRepository : BaseOracleRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OracleLicenseRepository(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(configuration)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LicenseDetailsResponse> GetOrgLicenseDetailsAsync(
        string civilId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // ✅ 1. Build Parameters
        var parameters = CreateParametersBuilder()
            .AddStringInput("P_LICN_CIVIL_ID", civilId)
            .AddStringInput("P_LICN_CENTRAL_NO", null)
            .AddStringInput("P_LICN_COMM_NO", null)
            .AddIntInput("P_LANG", 0)
            .AddStringInput("P_USER", userId)
            .AddRefCursors("P_OWNERS", "P_ACTIVITY", "P_LIC_DETAILS", "P_LIC_ADDRESS")
            .AddIntOutput("P_STATUS")
            .AddStringOutput("P_STATUS_DESC", 32767)
            .Build();

        // ✅ 2. استخدم الـ Method الجديد من BaseOracleRepository
        var (owners, activities, details, addresses) =
            await ExecuteQueryMultiple4Async<OwnerDto, ActivityDto, LicenseDetailsDto, AddressDto>(
                "APP_RE.RE_ORG_PKG.GET_ORG_LICN_DETAILS",
                parameters,
                cancellationToken
            );

        // ✅ 3. Read Output Parameters
        var status = parameters.Get<int>("P_STATUS");
        var statusDesc = parameters.Get<string>("P_STATUS_DESC");

        // ✅ 4. Build Response
        var response = new LicenseDetailsResponse
        {
            Status = status.ToString(),
            StatusDesc = statusDesc,
            Owners = owners.ToList(),
            Activities = activities.ToList(),
            LicenseDetails = details.FirstOrDefault(),
            Addresses = addresses.ToList()
        };

        // ✅ 5. حفظ في Session (لو محتاج)
        if (status == 1)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString("P_OWNERS", System.Text.Json.JsonSerializer.Serialize(response.Owners));
                session.SetString("P_ACTIVITY", System.Text.Json.JsonSerializer.Serialize(response.Activities));
                session.SetString("P_LIC_DETAILS", System.Text.Json.JsonSerializer.Serialize(response.LicenseDetails));
                session.SetString("P_LIC_ADDRESS", System.Text.Json.JsonSerializer.Serialize(response.Addresses));
            }
        }

        return response;
    }
}*/


/* prog
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});*/

/* session extension
 using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}*/


/* get session data
 * var owners = HttpContext.Session.Get<List<GET_ORG_LICN_OWNERS>>("P_OWNERS");
*/
