namespace kCura.IntegrationPoints.Email.Dto
{
    internal class ValidEmailAddress
    {
        public string Value { get; }

        internal ValidEmailAddress(string validEmailAdress)
        {
            Value = validEmailAdress;
        }

        public static implicit operator string(ValidEmailAddress validEmailAdress)
        {
            return validEmailAdress.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
