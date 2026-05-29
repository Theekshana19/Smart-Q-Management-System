using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullKioskTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "නව ගිණුම් විවෘත කිරීම, යාවත්කාලීන කිරීම් සහ ප්‍රකාශන ඉල්ලීම්.");

            migrationBuilder.UpdateData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "புதிய கணக்குகள், புதுப்பிப்புகள் மற்றும் அறிக்கை கோரிக்கைகள்.");

            migrationBuilder.InsertData(
                table: "ServiceTranslations",
                columns: new[] { "Id", "Description", "LanguageId", "Name", "ServiceId" },
                values: new object[,]
                {
                    { 5, "උපනිවේශ, පුද්ගලික ණය සහ ණය උපදේශන.", 2, "ණය සේවා", 3 },
                    { 6, "அடமானம், தனிப்பட்ட கடன் மற்றும் கடன் ஆலோசனை.", 3, "கடன் சேவைகள்", 3 },
                    { 7, "නව කාඩ්, PIN යළි පිහිටුවීම් සහ කාඩ් අවහිර කිරීම.", 2, "කාඩ් සේවා", 4 },
                    { 8, "புதிய அட்டை, PIN மீட்டமைப்பு மற்றும் அட்டை தடுப்பு.", 3, "அட்டை சேவைகள்", 4 },
                    { 9, "පොදු විමසීම්, පැමිණිලි සහ ඩිජිටල් බැංකු සහාය.", 2, "පාරිභෝගික සහාය", 5 },
                    { 10, "பொது விசாரணைகள், புகார்கள் மற்றும் டிஜிட்டல் வங்கி உதவி.", 3, "வாடிக்கையாளர் ஆதரவு", 5 }
                });

            migrationBuilder.InsertData(
                table: "SubServiceTranslations",
                columns: new[] { "Id", "Description", "LanguageId", "Name", "SubServiceId" },
                values: new object[,]
                {
                    { 1, "පුද්ගලික හෝ ව්‍යාපාරික ගිණුම්වලට මුදල් තැන්පත් කරන්න.", 2, "මුදල් තැන්පතු", 1 },
                    { 2, "தனிப்பட்ட அல்லது வணிகக் கணக்குகளில் பணம் வைக்கவும்.", 3, "பண வைப்பு", 1 },
                    { 3, "ATM සීමාවට වඩා වැඩි මුදල් නිකුත් කිරීම්.", 2, "මුදල් නිකුත් කිරීම", 2 },
                    { 4, "ATM வரம்பை விட அதிகமான பண எடுப்புகள்.", 3, "பண எடுப்பு", 2 },
                    { 5, "ගිණුම් අතර හෝ බාහිර ප්‍රතිලාභීන්ට මුදල් මාරු කිරීම.", 2, "මුදල් මාරු කිරීම", 3 },
                    { 6, "கணக்குகளுக்கு இடையில் அல்லது வெளிப்புறர்களுக்கு பணம் அனுப்புதல்.", 3, "பண பரிமாற்றம்", 3 },
                    { 7, "තරඟකාරී අනුපාතයන්හි විදේශ මුදල් මිලදී ගැනීම/විකිණීම.", 2, "විදේශ මුදල්", 4 },
                    { 8, "போட்டி விகிதங்களில் வெளிநாட்டு நாணயம் வாங்க/விற்.", 3, "வெளிநாட்டு நாணயம்", 4 },
                    { 9, "නව ඉතිරිකිරීම් හෝ ධාවක ගිණුමක් විවෘත කරන්න.", 2, "නව ගිණුම් විවෘත කිරීම", 5 },
                    { 10, "புதிய சேமிப்பு அல்லது நடப்புக் கணக்கைத் திறக்கவும்.", 3, "புதிய கணக்கு திறப்பு", 5 },
                    { 11, "ගිණුම් පැතිකඩ සහ සම්බන්ධතා තොරතුරු යාවත්කාලීන කරන්න.", 2, "ගිණුම් යාවත්කාලීන කිරීම", 6 },
                    { 12, "கணக்கு விவரங்களைப் புதுப்பிக்கவும்.", 3, "கணக்கு புதுப்பிப்பு", 6 },
                    { 13, "මුද්‍රිත හෝ ඩිජිටල් ගිණුම් ප්‍රකාශන ඉල්ලන්න.", 2, "ප්‍රකාශන ඉල්ලීම", 7 },
                    { 14, "அச்சு அல்லது டிஜிட்டல் அறிக்கையைக் கோருங்கள்.", 3, "அறிக்கை கோரிக்கை", 7 },
                    { 15, "මෑත ගනුදෙනු සහිත පාස්පොත යාවත්කාලීන කරන්න.", 2, "පාස්පොත් යාවත්කාලීනය", 8 },
                    { 16, "சமீபத்திய பரிவர்த்தனைகளுடன் பாஸ்புக் புதுப்பிக்கவும்.", 3, "பாஸ்புக் புதுப்பிப்பு", 8 },
                    { 17, "සාමාන්‍ය ණය තොරතුරු සහ සුදුසුකම් පරීක්ෂාව.", 2, "ණය විමසීම", 9 },
                    { 18, "பொது கடன் தகவல் மற்றும் தகுதி சரிபார்ப்பு.", 3, "கடன் விசாரணை", 9 },
                    { 19, "නව ණය අයදුමක් ඉදිරිපත් කරන්න.", 2, "ණය අයදුම", 10 },
                    { 20, "புதிய கடன் விண்ணப்பத்தைச் சமர்ப்பிக்கவும்.", 3, "கடன் விண்ணப்பம்", 10 },
                    { 21, "ණය වාරික ගෙවීම් කරන්න.", 2, "ණය ගෙවීම", 11 },
                    { 22, "கடன் தவணைகளைச் செலுத்துங்கள்.", 3, "கடன் செலுத்துதல்", 11 },
                    { 23, "ණය සම්බන්ධ ලේඛන ඉදිරිපත් කරන්න.", 2, "ලේඛන ඉදිරිපත් කිරීම", 12 },
                    { 24, "கடன் தொடர்பான ஆவணங்களைச் சமர்ப்பிக்கவும்.", 3, "ஆவண சமர்ப்பிப்பு", 12 },
                    { 25, "නව ඩෙබිට්/ක්‍රෙඩිට් කාඩ් ඉල්ලන්න.", 2, "නව කාඩ් ඉල්ලීම", 13 },
                    { 26, "புதிய டெபிட்/கிரெடிட் அட்டையைக் கோருங்கள்.", 3, "புதிய அட்டை கோரிக்கை", 13 },
                    { 27, "හානි වූ හෝ කල් ඉකුත් වූ කාඩ් ප්‍රතිස්ථාපනය.", 2, "කාඩ් ප්‍රතිස්ථාපනය", 14 },
                    { 28, "சேதமடைந்த அல்லது காலாவதியான அட்டைகளை மாற்றவும்.", 3, "அட்டை மாற்றம்", 14 },
                    { 29, "ශාඛාවේදී කාඩ් PIN යළි පිහිටුවන්න.", 2, "PIN යළි පිහිටුවීම", 15 },
                    { 30, "கிளையில் அட்டை PIN மீட்டமைக்கவும்.", 3, "PIN மீட்டமைப்பு", 15 },
                    { 31, "නැතිවූ හෝ සොරකම් කළ කාඩ් වාර්තා කරන්න.", 2, "නැතිවූ කාඩ් පැමිණිලි", 16 },
                    { 32, "இழந்த அல்லது திருடப்பட்ட அட்டையைப் புகாரளிக்கவும்.", 3, "இழந்த அட்டை புகார்", 16 },
                    { 33, "පොදු බැංකු විමසීම්.", 2, "පොදු විමසීම", 17 },
                    { 34, "பொது வங்கி விசாரணைகள்.", 3, "பொது விசாரணை", 17 },
                    { 35, "පාරිභෝගික පැමිණිල්ලක් ලියාපදිංචි කරන්න.", 2, "පැමිණිලි", 18 },
                    { 36, "வாடிக்கையாளர் புகாரைப் பதிவு செய்யுங்கள்.", 3, "புகார்", 18 },
                    { 37, "ඩිජිටල් බැංකු සහාය.", 2, "ඩිජිටල් බැංකුව", 19 },
                    { 38, "டிஜிட்டல் வங்கி உதவி.", 3, "டிஜிட்டல் வங்கி", 19 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "SubServiceTranslations",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.UpdateData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "නව ගිණුම් විවෘත කිරීම සහ ප්‍රකාශන ඉල්ලීම්.");

            migrationBuilder.UpdateData(
                table: "ServiceTranslations",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "புதிய கணக்குகள் மற்றும் அறிக்கை கோரிக்கைகள்.");
        }
    }
}
