namespace ServerLess_Zip.Util
{
    public static class ValidateEmailAddress
    {
        public static bool IsValidEmailAddress(string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }
    }
}
