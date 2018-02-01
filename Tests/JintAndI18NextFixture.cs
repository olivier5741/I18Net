using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Jint;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Text;

namespace Tests
{
    // TODO : is JINT thread safe
    
    [TestFixture]
    public class JintAndI18NextFixture
    {
        private Engine _engine;
        private ConcurrentDictionary<Tuple<string, string>, string> _cache;

        [Test]
        public void TranslateWithJintWithResourcesOnCDN()
        {
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            var i18next = "https://cdnjs.cloudflare.com/ajax/libs/i18next/8.1.0/i18next.min.js".GetStringFromUrl();

            _cache = new ConcurrentDictionary<Tuple<string,string>,string>();
            
            _engine = new Engine();
            _engine.Execute(i18next);

            var language = "en";

            var translationEn =
                "https://gist.githubusercontent.com/olivier5741/e6a61ba42a20d3225bd05665f8fb60aa/raw/770049a4cecc00cc418d96e8e3fa082429b185c8/main.en.json"
                    .GetJsonFromUrl().FromJson<Dictionary<string, object>>();

            var translationFr =
                "https://gist.githubusercontent.com/olivier5741/e6a61ba42a20d3225bd05665f8fb60aa/raw/770049a4cecc00cc418d96e8e3fa082429b185c8/main.fr.json"
                    .GetJsonFromUrl().FromJson<Dictionary<string, object>>();

            _engine.SetValue("translationEn", translationEn);
            _engine.SetValue("translationFr", translationFr);

            _engine.Execute(@"
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

            _engine.Execute("function tLanguage(p, l) { return i18next.t(p,{lng: l}); }");
            
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                var s = TranslateAndCache("input.placeholder", "en");
            }
 
            sw.Stop();
            
            sw.ElapsedMilliseconds.PrintDump(); 
            /*
             * On local machine inside NUnit
             * 1 -> 25ms
             * 10 -> 31ms
             * 100 -> 73ms
             * 1000 -> 601ms
             * 10000 -> 6508ms but 25ms when caching :)
             */

            Assert.AreEqual("a placeholder",Translate("input.placeholder", "en"));
            Assert.AreEqual("tapez voter texte",Translate("input.placeholder", "fr"));
            
            // caching
            Assert.AreEqual("a placeholder",TranslateAndCache("input.placeholder", "en"));
            Assert.AreEqual("tapez voter texte",TranslateAndCache("input.placeholder", "fr"));
        }

        private string Translate(string property, string language)
        {
            return _engine.Invoke("tLanguage", property, language).AsString();
        }
        
        private string TranslateAndCache(string property, string language)
        {
            return _cache.GetOrAdd(new Tuple<string, string>(property, language),
                key => _engine.Invoke("tLanguage", property, language).AsString());
        }
    }
}