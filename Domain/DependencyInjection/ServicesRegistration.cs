﻿using Microsoft.Extensions.DependencyInjection;

using Domain.Factories;
using Domain.IO;
using Domain.Services;

namespace Domain.DependencyInjection;

public static class ServicesRegistration
{
    public static IServiceCollection RegisterDomainClasses(this IServiceCollection services)
    {
        return services
            // Facades
            .AddScoped<IMagicProxyPrinter, MagicProxyPrinter>()
            // Factories
            .AddScoped<IDeckRetrieverFactory, DeckRetrieverFactory>()
            // IO
            .AddScoped<ICardListFileParser, CardListFileParser>()
            .AddScoped<IFileManager, FileManager>()
            .AddScoped<IWordDocumentWrapper, WordDocumentWrapper>()
            // Services
            .AddScoped<IArchidektService, ArchidektService>()
            .AddScoped<IEdhrecService, EdhrecService>()
            .AddScoped<IMoxfieldService, MoxfieldService>()
            .AddScoped<IScryfallService, ScryfallService>()
            .AddScoped<ILanguageService, LanguageService>()
            .AddScoped<IWordGeneratorService, WordGeneratorService>()
        ;
    }
}