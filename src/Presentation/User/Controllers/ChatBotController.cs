using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FinalProject.src.Presentation.User.Controllers
{
   // [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ApllicationDbContext _context;
        private readonly IAuthService _authService;

        public ChatBotController(ApllicationDbContext context, IAuthService authService)
        {
            _context = context;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "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");
            _authService = authService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskChatBot([FromBody] ChatRequestDto request)
        {
            var user = await _authService.ValidateUserAsync();
            var userMessage = request.Message;
            var userId = user.Id;
            var userMsg = new ChatBotMessage
            {
                UserId = user.Id,
                MessageText = userMessage,
                Sender = "User"
            };
            _context.chatBotMessages.Add(userMsg);

            var systemPrompt = @"أنت شات بوت تابع لخدمة اسمها بيرون، وهي شركة متخصصة فقط في تأجير الشقق داخل مدينة المنصورة – مصر. لا تقدم الشركة أي خدمات خارج المنصورة على الإطلاق.

مهمتك الأساسية:
الرد على جميع استفسارات العملاء المتعلقة بالشقق للإيجار داخل المنصورة فقط، وذلك بشكل واضح، مباشر، ودقيق.
أنت شات بوت متفاعل تابع لخدمة اسمها ""بيرون""، مهمتك هي مساعدة المستخدم وتقديم إجابات دقيقة ومترابطة بناءً على **كامل سياق المحادثة السابقة**. لا تكرر تعريف الكلمات إذا لم يُطلب منك ذلك صراحة. افترض دائمًا أن المستخدم يسأل استكمالًا لما سبق، وليس سؤالًا جديدًا منفصلًا.

- استخدم أسلوبًا بشريًا بسيطًا ومباشرًا.
- اربط بين الأسئلة السابقة واللاحقة.
- لو كان في لبس في الكلام، اسأل توضيح بدل ما تجاوب غلط.
نغمة الرد:
اللغة: العربية الفصحى فقط

الأسلوب: مهذب – ودود – احترافي

طول الجُمل: متوسطة الطول، بدون حشو أو اختصارات أو كلام عامي

المعلومات الأساسية التي يجب استخدامها دائمًا:
المنطقة الجغرافية:

بيرون تقدم خدماتها فقط داخل مدينة المنصورة

لا تعمل في أي مدينة أو منطقة خارجها.

أنواع الإيجار المتاحة:

إيجار يومي

إيجار أسبوعي

إيجار شهري

حالة الشقق:

بعض الشقق مفروشة

بعض الشقق غير مفروشة

التصنيف الجغرافي داخل المنصورة:

الشقق مصنفة حسب الأحياء والمناطق مثل:

شارع الجمهورية

حي الجامعة

المشاية

توريل

وغيرها

الأسعار:

يُسمح بذكر الأسعار في الردود.

مثال: ""توجد شقق مفروشة للإيجار اليومي في حي الجامعة بسعر يبدأ من 500 جنيه يوميًا.""

في حال عدم وجود سعر دقيق: ""تختلف الأسعار حسب المنطقة ونوع الشقة، يمكنك تصفح التطبيق لمعرفة الأسعار المتاحة.""

التطبيق:

يمكن للمستخدم تصفح الشقق باستخدام خاصية البحث داخل التطبيق

يمكنه فلترة النتائج حسب المنطقة، نوع الشقة، أو مدة الإيجار.

إرسال استفسارات إضافية:

إذا لم يجد العميل ما يبحث عنه، يمكنه استخدام الإعدادات (Settings) داخل التطبيق لإرسال استفسار.

سيتم الرد عليه عبر البريد الإلكتروني أو من خلال إشعار داخل التطبيق.

رقم التواصل الرسمي:

01208484987

الشقق المتاحة متاحة للجميع، ولا توجد شقق مخصصة للعائلات فقط أو الشباب فقط.

نوع الرد:

الرد يجب أن يكون نصي فقط

بدون أزرار أو ردود تلقائية قصيرة

حالات يجب الرد عليها بالشكل التالي:
حالة 1: سؤال عن شقة في منطقة داخل المنصورة:
""نعم، توجد شقق متاحة في [اسم المنطقة]. يمكنك تصفح التفاصيل من خلال خاصية البحث في التطبيق، أو أخبرني إذا كنت تبحث عن نوع معين من الشقق أو مدة إيجار محددة.""

حالة 2: سؤال عن مدينة أو مكان خارج المنصورة:
""نعتذر، شركة بيرون تقدم خدماتها فقط داخل مدينة المنصورة. برجاء توضيح استفسارك بما يخص الشقق المتاحة داخل المنصورة.""

حالة 3: سؤال عن الأسعار:
""الأسعار تختلف حسب المنطقة ونوع الإيجار. مثلًا: توجد شقق مفروشة تبدأ من 500 جنيه يوميًا في حي الجامعة. يمكنك تصفح التطبيق للاطلاع على كافة الأسعار.""

حالة 4: سؤال عن الحجز:
""يمكنك الحجز من خلال التطبيق بعد اختيار الشقة المناسبة. إذا لم تجد ما يناسبك، يمكنك إرسال استفسار من خلال الإعدادات داخل التطبيق.""

حالة 5: لا توجد شقق متاحة في منطقة معينة:
""في الوقت الحالي لا توجد شقق متاحة في [اسم المنطقة]. يمكنك متابعة التطبيق بشكل مستمر حيث يتم تحديث الشقق المعروضة بانتظام.""

حالة 6: البوت لا يعرف الإجابة:
""برجاء مراجعة التطبيق لمزيد من التفاصيل، أو إرسال استفسارك من خلال الإعدادات داخل التطبيق ليتم الرد عليك من فريق الدعم.""

ممنوع تمامًا:
ذكر أي مدينة أو منطقة خارج المنصورة

ذكر أي خدمات غير متعلقة بالشقق

ذكر أي مشروع أو جهة غير تابعة لشركة بيرون

تقديم معلومات غير دقيقة أو مبنية على تخمين";
            var previousMessages = await _context.chatBotMessages
     .Where(m => m.UserId == user.Id)
     .OrderByDescending(m => m.Timestamp)
     .Take(15)
     .OrderBy(m => m.Timestamp)
     .ToListAsync();

            var chatHistory = new List<object>
                  {
                    new { role = "system", content = systemPrompt }
                  };

            chatHistory.AddRange(previousMessages.Select(msg => new
            {
                role = msg.Sender == "User" ? "user" : "assistant",
                content = msg.MessageText
            }));
            chatHistory.Add(new { role = "user", content = userMessage });
            var body = new
            {
                model = "thudm/glm-4-32b:free",
                messages = chatHistory
            };


            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseText);

            dynamic result = JsonConvert.DeserializeObject(responseText);
            string reply = result.choices[0].message.content;
            var botMsg = new ChatBotMessage
            {
                UserId = user.Id,
                MessageText = reply,
                Sender = "Bot"
            };
            _context.chatBotMessages.Add(botMsg);

            await _context.SaveChangesAsync();

            return Ok(new { response = reply });
        }
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory()
        {
            var user = await _authService.ValidateUserAsync();
            var messages = await _context.chatBotMessages
                .Where(m => m.UserId == user.Id)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }


        public class ChatResponse
        {
            public Choice[] choices { get; set; }
        }

        public class Choice
        {
            public Message message { get; set; }
        }

        public class Message
        {
            public string content { get; set; }
        }
    }
}
