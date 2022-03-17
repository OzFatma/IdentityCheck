using IdentityCheck.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCheck.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChecksController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(long tc, string name, string surname, int birth)
        {

            if (Kontrol(tc.ToString()))
            {
                bool dogrulamaSonucu = false;
                try
                {
                    var client = new KPSPublic.KPSPublicSoapClient(KPSPublic.KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
                    var tcKimlikDogrulamaServisResponse = await client.TCKimlikNoDogrulaAsync(tc, name, surname, birth);
                    dogrulamaSonucu = tcKimlikDogrulamaServisResponse.Body.TCKimlikNoDogrulaResult;
                }
                catch (Exception ex)
                {
                    dogrulamaSonucu = false;
                }
                if (dogrulamaSonucu) { return Ok("Sorgulama doğru"); }

            }
            return BadRequest("Hatalı sorgu");
        }

        public static bool Kontrol(string TCno)
        {
            int Algoritma_Adim_Kontrol = 0, TekBasamaklarToplami = 0, CiftBasamaklarToplami = 0;

            if (TCno.Length != 11) return false;


            Algoritma_Adim_Kontrol = 1;
            foreach (char chr in TCno) { if (Char.IsNumber(chr)) Algoritma_Adim_Kontrol = 2; }
            if (TCno.Substring(0, 1) != "0") Algoritma_Adim_Kontrol = 3;

            int[] arrTC = System.Text.RegularExpressions.Regex.Replace(TCno, "[^0-9]", "").Select(x => (int)Char.GetNumericValue(x)).ToArray();

            for (int i = 0; i < TCno.Length; i++)
            {
                if (((i + 1) % 2) == 0)
                    if (i + 1 != 10) CiftBasamaklarToplami += Convert.ToInt32(arrTC[i]);
                    else
                    if (i + 1 != 11) TekBasamaklarToplami += Convert.ToInt32(arrTC[i]);
            }

            if (Convert.ToInt32(TCno.Substring(9, 1)) == (((TekBasamaklarToplami * 7) - CiftBasamaklarToplami) % 10)) Algoritma_Adim_Kontrol = 4;
            if (Convert.ToInt32(TCno.Substring(10, 1)) == ((arrTC.Sum() - Convert.ToInt32(TCno.Substring(10, 1))) % 10)) Algoritma_Adim_Kontrol = 5;

            if (Algoritma_Adim_Kontrol == 5)
                return true;
            else
                return false;
        }
    }
}
