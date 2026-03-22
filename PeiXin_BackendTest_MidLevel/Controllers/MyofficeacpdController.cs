using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using PeiXin_BackendTest_MidLevel.Models;

namespace PeiXin_BackendTest_MidLevel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyofficeacpdController : ControllerBase
    {
        private readonly string _connectionString;

        public MyofficeacpdController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// 取得所有 MyOffice_ACPD 資料的 API
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MyOfficeAcpd>>> GetAll()
        {
            using var conn = new SqlConnection(_connectionString);
            return Ok(await conn.QueryAsync<MyOfficeAcpd>("SELECT * FROM MyOffice_ACPD"));
        }

        /// <summary>
        /// 取得單筆 MyOffice_ACPD 資料的 API，根據 ACPD_SID 查詢
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<MyOfficeAcpd>> GetById(string id)
        {
            using var conn = new SqlConnection(_connectionString);
            var result = await conn.QueryFirstOrDefaultAsync<MyOfficeAcpd>(
                "SELECT * FROM MyOffice_ACPD WHERE ACPD_SID = @id", new { id });
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// 新增 MyOffice_ACPD 資料的 API，需檢查 Email 是否已存在，並使用自動命名的 SP 取得主鍵值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<MyOfficeAcpd>> Create([FromBody] MyOfficeAcpd model)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var emailExists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM MyOffice_ACPD WHERE ACPD_Email = @ACPD_Email",
                new { model.ACPD_Email });

            if (emailExists > 0)
            {
                return Conflict(new { message = "該 Email 已被使用" });
            }

            var parameters = new DynamicParameters();
            parameters.Add("@TableName", "MyOffice_ACPD", DbType.String);
            parameters.Add("@ReturnSID", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
            await conn.ExecuteAsync("NEWSID", parameters, commandType: CommandType.StoredProcedure);

            model.ACPD_SID = parameters.Get<string>("@ReturnSID");

            var sql = @"INSERT INTO MyOffice_ACPD 
                        (ACPD_SID, ACPD_Cname, ACPD_Ename, ACPD_Sname, ACPD_Email, ACPD_Status, ACPD_Stop, ACPD_LoginID, ACPD_LoginPWD, ACPD_Memo, ACPD_NowDateTime, ACPD_UPDDateTime)
                        VALUES 
                        (@ACPD_SID, @ACPD_Cname, @ACPD_Ename, @ACPD_Sname, @ACPD_Email, @ACPD_Status, @ACPD_Stop, @ACPD_LoginID, @ACPD_LoginPWD, @ACPD_Memo, GETDATE(), GETDATE())";

            await conn.ExecuteAsync(sql, model);
            return CreatedAtAction(nameof(GetById), new { id = model.ACPD_SID }, model);
        }

        /// <summary>
        /// 修改 MyOffice_ACPD 資料的 API，需檢查該 SID 是否存在，以及 Email 是否被其他帳號佔用（排除掉自己目前的 SID）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MyOfficeAcpd model)
        {
            using var conn = new SqlConnection(_connectionString);

            var exists = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM MyOffice_ACPD WHERE ACPD_SID = @id", new { id });
            if (exists == 0) return NotFound();

            var emailConflict = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM MyOffice_ACPD WHERE ACPD_Email = @ACPD_Email AND ACPD_SID <> @id",
                new { model.ACPD_Email, id });

            if (emailConflict > 0)
            {
                return Conflict(new { message = "此 Email 已被其他人員使用" });
            }

            var sql = @"UPDATE MyOffice_ACPD SET 
                        ACPD_Cname = @ACPD_Cname, 
                        ACPD_Ename = @ACPD_Ename, 
                        ACPD_Email = @ACPD_Email,
                        ACPD_Status = @ACPD_Status, 
                        ACPD_UPDDateTime = GETDATE()
                        WHERE ACPD_SID = @id";

            await conn.ExecuteAsync(sql, new { id, model.ACPD_Cname, model.ACPD_Ename, model.ACPD_Email, model.ACPD_Status });
            return Ok(new { message = "Update Success" });
        }

        /// <summary>
        /// 刪除 MyOffice_ACPD 資料的 API，根據 ACPD_SID 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using var conn = new SqlConnection(_connectionString);
            var rowsAffected = await conn.ExecuteAsync("DELETE FROM MyOffice_ACPD WHERE ACPD_SID = @id", new { id });
            if (rowsAffected == 0) return NotFound();
            return NoContent();
        }
    }
}