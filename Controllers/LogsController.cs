using dy.net.dto;
using dy.net.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace dy.net.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly DouyinHttpClientService douyinHttpClientService;

        public LogsController(IWebHostEnvironment webHostEnvironment,DouyinHttpClientService douyinHttpClientService)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.douyinHttpClientService = douyinHttpClientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLog(string type, string date)
        {
            var filePath = Path.Combine(webHostEnvironment.IsDevelopment() ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-{type}-{date}.txt");
            if (!System.IO.File.Exists(filePath))
            {
                var msg = $"Log file log-{type}-{date}.txt not found.";
                //Serilog.Log.Error(msg);
                return Ok(msg);
            }
            return PhysicalFile(filePath, "text/plain; charset=utf-8");

            //下面的方案提示文件被占用
            //var encoding = Encoding.GetEncoding("UTF-8"); // 指定文本文件的编码
            //var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //var fileContent = encoding.GetString(fileBytes);
            //return  Content (fileContent, "text/plain", encoding);
        }
        ///// <summary>
        ///// 测试
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> TestHttpClient()
        //{
        //    string count = "20";
        //    string offset = "0";
        //    string cookie = "";
        //    bool hasmore = true;

        //    {
        //        cookie = "gfkadpd=6383,41079; passport_csrf_token=a5da631bccfeddbc877bd4e1f72f142f; passport_csrf_token_default=a5da631bccfeddbc877bd4e1f72f142f; enter_pc_once=1; UIFID_TEMP=0e81ba593d64ebaca259bdbe302de8d7e55ac2e982f7412f10fbc5c77c64bb8b8830ffed112ea8b3bc54f3b6e2af3856daafa4efaafef18953612820da493aea7fe297beed13f9b141a7a3100c4d9aa7d1428f23d05cf5979bf209937ba9ad00da1efa59922c98942d4ce449e736ee2b; x-web-secsdk-uid=849eef8b-171b-4bd7-9cce-5752f14e53fd; douyin.com; device_web_cpu_core=32; device_web_memory_size=8; architecture=amd64; hevc_supported=true; dy_swidth=1707; dy_sheight=1067; s_v_web_id=verify_migsr5je_BJ1YiVbY_uR2U_4VXu_8Uje_GgwhVf7Y6hb8; fpk1=U2FsdGVkX19wNXQ61AQVcAXPKhYMylJU4AcD8JGkFeNZmB4mYB1XNB+bc+hU1lhq6ZruxA+I9Nwv4EeNoW414w==; fpk2=df46e1d3e7507fa3d6888a71a1894105; strategyABtestKey=%221764209447.927%22; volume_info=%7B%22isUserMute%22%3Afalse%2C%22isMute%22%3Afalse%2C%22volume%22%3A0.5%7D; xgplayer_user_id=90669686736; bd_ticket_guard_client_web_domain=2; is_dash_user=1; n_mh=Bdbeto8B8DXIgXtXZG-7Vw3HWofM6NyG-v3Wgk1pL6s; is_staff_user=false; __security_server_data_status=1; publish_badge_show_info=%220%2C0%2C0%2C1764209517482%22; UIFID=0e81ba593d64ebaca259bdbe302de8d7e55ac2e982f7412f10fbc5c77c64bb8b8830ffed112ea8b3bc54f3b6e2af3856daafa4efaafef18953612820da493aeaebdeac0537b99b8e68f343c2476cf6347f7751a87d92952128116470f147215cf46b949f43f31aedcb660fd3c5c7eb2145183cc93d1b4202205b4af7d7c69ab55e860f96e315899a2ee74a262273694ec973ba682bb6fdc351e0e66250b21cf9aef3ccaa3e0c28b085fd095e947d92a5336b9e65706d649a7b79541feab3487f; SelfTabRedDotControl=%5B%5D; xg_device_score=7.708466147936333; my_rd=2; stream_player_status_params=%22%7B%5C%22is_auto_play%5C%22%3A0%2C%5C%22is_full_screen%5C%22%3A0%2C%5C%22is_full_webscreen%5C%22%3A0%2C%5C%22is_mute%5C%22%3A0%2C%5C%22is_speed%5C%22%3A1%2C%5C%22is_visible%5C%22%3A0%7D%22; ttwid=1%7CffZO3FHfC8TRR6jdc7mfx3iFnrCqB_UuViDs3Lvj484%7C1764213923%7C12e8625d232e8a4ef61fc36807cbbf75d47e741e1e3ee82818ce82fc95612188; passport_mfa_token=Cjdg9xehkrlYj4Y1CWlAlHwJkHR2vpgE2ofiFeM3C8Zinm%2FK4vD%2BS%2Fk4k0uLAYrRsB3jJ8xoZlY6GkoKPAAAAAAAAAAAAABPwiYdUJ8TJRCoJEg7%2BU1U9xJn8%2FiVx37dL0fj0JsjXpjYp%2BFLThU1gLHtftqV3kgk4RDl0IIOGPax0WwgAiIBA3w4KGo%3D; d_ticket=b5badc9fd18022a5dd711f9f79bd33bf33b59; passport_assist_user=CkGOoW38C6tHKDGFYJ-Az5SHV8tHsa8HLiQy3uxuO_Qxf_gBNX1gwhyZd9i2pSAqGqUDpE5z3XSLYripy9Qf2KRRlRpKCjwAAAAAAAAAAAAAT8M736Jo4z6WK0kchkJMfk7WgAGu6ZEzrFSguCH7TfcT0niPHFmH_gIzWAEnmo8pqD4QmNKCDhiJr9ZUIAEiAQO-WDqX; sid_guard=a8f80e93207ab87a7128acf6da58e379%7C1764213945%7C5183999%7CMon%2C+26-Jan-2026+03%3A25%3A44+GMT; uid_tt=339c6ee64effdce094392ef399cb7ecc; uid_tt_ss=339c6ee64effdce094392ef399cb7ecc; sid_tt=a8f80e93207ab87a7128acf6da58e379; sessionid=a8f80e93207ab87a7128acf6da58e379; sessionid_ss=a8f80e93207ab87a7128acf6da58e379; session_tlb_tag=sttt%7C9%7CqPgOkyB6uHpxKKz22ljjef_________B-B1oMrODP-ItQszjCwbW8eVitD4IIGm5p5M8HZ9mUmY%3D; session_tlb_tag_bk=sttt%7C9%7CqPgOkyB6uHpxKKz22ljjef_________B-B1oMrODP-ItQszjCwbW8eVitD4IIGm5p5M8HZ9mUmY%3D; sid_ucp_v1=1.0.0-KDA1Mzc3NGFlYjkxMWVhYmYwMjRkYjFlNzlmOTExMTJjNDU4YmMwYWEKIQinwqCz24yVAhC5iZ_JBhjvMSAMMLHN7ZIGOAVA-wdIBBoCbHEiIGE4ZjgwZTkzMjA3YWI4N2E3MTI4YWNmNmRhNThlMzc5; ssid_ucp_v1=1.0.0-KDA1Mzc3NGFlYjkxMWVhYmYwMjRkYjFlNzlmOTExMTJjNDU4YmMwYWEKIQinwqCz24yVAhC5iZ_JBhjvMSAMMLHN7ZIGOAVA-wdIBBoCbHEiIGE4ZjgwZTkzMjA3YWI4N2E3MTI4YWNmNmRhNThlMzc5; login_time=1764213943926; _bd_ticket_crypt_cookie=8557ac65ef240b5ae1ef4f42fc295a33; download_guide=%223%2F20251127%2F0%22; stream_recommend_feed_params=%22%7B%5C%22cookie_enabled%5C%22%3Atrue%2C%5C%22screen_width%5C%22%3A1707%2C%5C%22screen_height%5C%22%3A1067%2C%5C%22browser_online%5C%22%3Atrue%2C%5C%22cpu_core_num%5C%22%3A32%2C%5C%22device_memory%5C%22%3A8%2C%5C%22downlink%5C%22%3A10%2C%5C%22effective_type%5C%22%3A%5C%224g%5C%22%2C%5C%22round_trip_time%5C%22%3A0%7D%22; __ac_signature=_02B4Z6wo00f01W7nUdgAAIDCMXJaFv5vWAFux1VAADKh6e; home_can_add_dy_2_desktop=%221%22; bd_ticket_guard_client_data=eyJiZC10aWNrZXQtZ3VhcmQtdmVyc2lvbiI6MiwiYmQtdGlja2V0LWd1YXJkLWl0ZXJhdGlvbi12ZXJzaW9uIjoxLCJiZC10aWNrZXQtZ3VhcmQtcmVlLXB1YmxpYy1rZXkiOiJCRmRYMTNGQjNZRC9jckpRMjRnbElqdVg3SFBINm5pSW5KSHVwczFkOWZYcTN6RG12RnZiNGxxVDhMOXkwV3dZRzZaVTJIZVZ5ZkFoMVBhRUJ6cGtGamc9IiwiYmQtdGlja2V0LWd1YXJkLXdlYi12ZXJzaW9uIjoyfQ%3D%3D; FOLLOW_NUMBER_YELLOW_POINT_INFO=%22MS4wLjABAAAAfHeXHAcMODTRd6RzfhNvGcNo9jzHveOylmtvMnmHQ6aeaM9kHHj9Oz5hffXp4TGT%2F1764259200000%2F0%2F1764220136874%2F0%22; odin_tt=edd4b1d4aae751ee0896cc364174559a12d84ce1f8b6a1da99790965bfb2bf9683312194f434f1f0b7226ff385ad0a5c3af1db88c3c746cb0e9a303dc8613b74; biz_trace_id=831e7c1c; WallpaperGuide=%7B%22showTime%22%3A1764223033231%2C%22closeTime%22%3A0%2C%22showCount%22%3A1%2C%22cursor1%22%3A8%2C%22cursor2%22%3A2%7D; FOLLOW_LIVE_POINT_INFO=%22MS4wLjABAAAAfHeXHAcMODTRd6RzfhNvGcNo9jzHveOylmtvMnmHQ6aeaM9kHHj9Oz5hffXp4TGT%2F1764259200000%2F1764217238572%2F1764223036809%2F0%22; sdk_source_info=7e276470716a68645a606960273f276364697660272927676c715a6d6069756077273f276364697660272927666d776a68605a607d71606b766c6a6b5a7666776c7571273f275e58272927666a6b766a69605a696c6061273f27636469766027292762696a6764695a7364776c6467696076273f275e582729277672715a646971273f2763646976602729277f6b5a666475273f2763646976602729276d6a6e5a6b6a716c273f2763646976602729276c6b6f5a7f6367273f27636469766027292771273f273c31333c303d35363731333234272927676c715a75776a716a666a69273f2763646976602778; bit_env=nl1wMQMlK740nH9CwNwtw9nW-axyI9dJrxf5kgYmELm5pzFHljUvSp4oQndC0NkuxnTy87qC0dvnETYah7B-A9apPsdLbcfSiM_Uut6c8ANQfqKhlvzSEI3nFSVsgT96_RHqMUfrRNUa1i6TZa917AZaz3WHrPF_5nlLzsF95CAlBpCxiI-W9wdKa5lMissqiUhAY4CRTEoGuXu2CuDqgrgI9WJ6cFngoB27EjQYAYt1k-K45Me1qCHbeJkPwuy7mA8DT9hJVrK7z3iOaw-ObHU1HL-al8AxFVAf6sfyIh_arjwxjG4ebl1wU6bkqyIYM7dgfo_Gdp-XzX8DbI5fUBGKsI7NSw9DO2rqpd4bdex4j-ZAqj4uK_KsCs4Uc1jRuMKRCps8d9AW_rc4AXPQHzI7b8-cE5GNomFe1lt4aFcnaNIAsmW77_Fax1PW3c8YeIbrjMKJ_RkJNkjs36nWTDf-W-Qt2gutul5cQXb18Isw5OBNyJM9cviGL7xJNsGLXl7gnfvgOswxAR3CvYActmevtbp3bBHQJnmFTlw_mSg%3D; gulu_source_res=eyJwX2luIjoiZjI1NzFkMzg0MDZkYWFhM2I1MGFkY2E0MjgxMDI4N2VmMDEwMDcxYjQzNTA2ZWJkY2RlOGYxZDZmMjYyZWQ0NCJ9; passport_auth_mix_state=yf0zr26dxo77w4o42cdt2f2w1ix5qz8q; __security_mc_1_s_sdk_crypt_sdk=3add21b7-4ccc-b199; __security_mc_1_s_sdk_cert_key=9e23dfb2-40a6-8498; __security_mc_1_s_sdk_sign_data_key_web_protect=a7f31503-424a-871a; bd_ticket_guard_client_data_v2=eyJyZWVfcHVibGljX2tleSI6IkJGZFgxM0ZCM1lEL2NySlEyNGdsSWp1WDdIUEg2bmlJbkpIdXBzMWQ5ZlhxM3pEbXZGdmI0bHFUOEw5eTBXd1lHNlpVMkhlVnlmQWgxUGFFQnpwa0ZqZz0iLCJ0c19zaWduIjoidHMuMi45NWViZjU5NzE3NzBkMTk3NTgxOWNhMzYyNTAzNGZhMzk3NWYyMTY2M2MwNmY0Yjk2MjZiNjhhNzgwMzEwMTJiYzRmYmU4N2QyMzE5Y2YwNTMxODYyNGNlZGExNDkxMWNhNDA2ZGVkYmViZWRkYjJlMzBmY2U4ZDRmYTAyNTc1ZCIsInJlcV9jb250ZW50Ijoic2VjX3RzIiwicmVxX3NpZ24iOiJEeElacmxmS3ROL2lzSytGRFZ3czNDaHQvTEt0L2dDazRQa0ZaMnJEVmpRPSIsInNlY190cyI6IiNjM3BLcFhDREpvczUxak9JUGw2RlMrNEtuWjllOVRGTy9KT2Qrdk1LZ3VtZk5WYm5Ob2s3QlRVNmprc2MifQ%3D%3D; IsDouyinActive=tru";
        //    }

        //    List<FollowingsItem> followings = new List<FollowingsItem>();
        //    while (hasmore)
        //    {
        //        var response = await douyinHttpClientService.SyncMyFollows(count, offset, "MS4wLjABAAAAfHeXHAcMODTRd6RzfhNvGcNo9jzHveOylmtvMnmHQ6aeaM9kHHj9Oz5hffXp4TGT", cookie);

        //        hasmore = response.HasMore;

        //        if(response.Followings != null)
        //        {
        //            offset = response.Offset.ToString();
        //            foreach (var item in response.Followings)
        //            {
        //                followings.Add(item);
        //            }
        //        }
        //    }

        //    return Ok(followings);
        //}


    }
}
