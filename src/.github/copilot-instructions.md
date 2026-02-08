# Copilot Instructions

## General Guidelines
- Use custom .NotEmpty("message") validation convention instead of .NotEmpty().WithMessage("message") in FluentValidation validators. The Goodtocode.Validation library extends FluentValidation with custom syntax where validation methods accept message as a parameter: .NotEmpty("message"), .NotEqual(value, "message"), etc. Do not use .WithMessage() - pass the message directly as a parameter to the validation method.