using System;
using System.Collections.Generic;
using System.Diagnostics;
using Jint;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Text;

namespace Tests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Test1()
        {
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            var i18next = "https://cdnjs.cloudflare.com/ajax/libs/i18next/8.1.0/i18next.min.js".GetStringFromUrl();

            var engine = new Engine();
            engine.Execute(i18next);

            var language = "en";

            var translationEn =
                "https://gist.githubusercontent.com/olivier5741/e6a61ba42a20d3225bd05665f8fb60aa/raw/770049a4cecc00cc418d96e8e3fa082429b185c8/main.en.json"
                    .GetJsonFromUrl().FromJson<Dictionary<string, object>>();

            var translationFr =
                "https://gist.githubusercontent.com/olivier5741/e6a61ba42a20d3225bd05665f8fb60aa/raw/770049a4cecc00cc418d96e8e3fa082429b185c8/main.fr.json"
                    .GetJsonFromUrl().FromJson<Dictionary<string, object>>();

            engine.SetValue("translationEn", translationEn);
            engine.SetValue("translationFr", translationFr);

            engine.Execute(@"
              i18next.init({
                lng: 'en',
                resources: { 
                  en: {
                    translation: translationEn
                  },
                  fr: {
                    translation: translationFr
                  }
                }
              });");

            engine.Execute("function tLanguage(p, l) { return i18next.t(p,{lng: l}); }");
            
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 1; i++)
            {
                var s = Translate(engine, "input.placeholder", "en");
            }
 
            sw.Stop();
            
            sw.ElapsedMilliseconds.PrintDump(); 
            /*
             * On local machine inside NUnit
             * 1 -> 25ms
             * 10 -> 31ms
             * 100 -> 73ms
             * 1000 -> 601ms
             * 10000 -> 6508ms
             */

            Assert.AreEqual("a placeholder",Translate(engine, "input.placeholder", "en"));
            
            Assert.AreEqual("tapez voter texte",Translate(engine, "input.placeholder", "fr"));
        }

        private static string Translate(Engine engine, string property, string language)
        {
            return engine.Invoke("tLanguage", property, language).AsString();
        }
    }
}