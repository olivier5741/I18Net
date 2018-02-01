using System;
using System.Collections.Generic;
using Jint;
using NUnit.Framework;
using ServiceStack;

namespace Tests
{
    
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Test1()
        {
            var i18next = "https://cdnjs.cloudflare.com/ajax/libs/i18next/8.1.0/i18next.min.js".GetStringFromUrl();
            
            var engine = new Engine();
            engine.Execute(i18next);

            engine.Execute(@"
              i18next.init({
                lng: 'en',
                resources: { 
                  en: {
                    translation: {
                      input: {
                        placeholder: 'a placeholder'
                    },
                    nav: {
                        home: 'Home',
                        page1: 'Page One',
                        page2: 'Page Two'
                    }
                }
            }
            }
            }, function(err, t) {
            });");
            
            engine.Execute("var trans = i18next.t('input.placeholder');");
            
            Assert.AreEqual("a placeholder",engine.GetValue("trans").AsString());
        }
    }
}