namespace Core.Extensions
{
    public static class StringExtensions
    {
        private const string ControllerSuffix = "Controller";

        public static string WithoutControllerSuffix(this string controllerName)
        {
            if (!controllerName.EndsWith(ControllerSuffix))
                return controllerName;

            var index = controllerName.LastIndexOf(ControllerSuffix);
            return controllerName[..index];
        }
    }
}
