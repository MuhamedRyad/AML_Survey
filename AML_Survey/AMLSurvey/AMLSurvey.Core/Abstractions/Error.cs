

namespace AMLSurvey.Core.Abstractions
{
    public record Error(string Code, string Description, int? StatusCode)
    {
        // empty object 
        public static readonly Error None = new(string.Empty, string.Empty, null);
        // common errors
       /*
        public static readonly Error Unknown = new("UNKNOWN", "An unknown error occurred.", StatusCodes.Status500InternalServerError);
        public static readonly Error NotFound = new("NOT_FOUND", "العنصر غير موجود", StatusCodes.Status404NotFound);
        public static readonly Error Unauthorized = new("UNAUTHORIZED", "الوصول غير مسموح", StatusCodes.Status401Unauthorized);
       */
        // Example of custom error
        /*var e1 = new Error("PollNotFound", "الاستبيان غير موجود", 404);

         // readonly static instance of Error
        var e2 = Error.None;*/
    }
}
