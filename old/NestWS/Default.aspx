<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>NEST Web Service</title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>NEST Web Service</h1>
        <p>
            Služba se nachází na adrese <a href="http://nestws.aspweb.cz/nestws.asmx">http://nestws.aspweb.cz/nestws.asmx</a></p>
        <h2>
            Obsah </h2>
        <ul>
            <li><a href="#dokumentace">Dokumentace</a></li>
            <ul>
            <li><a href="#Function RunConsultation">Function RunConsultation</a>
            <ul>
            <li><a href="#Parametry">Parametry</a></li>
                <li><a href="#Syntaxe BazeZnalostiXML">Syntaxe BazeZnalostiXML</a></li>
                <li><a href="#Syntaxe AnswersXML">Syntaxe AnswersXML</a></li>
                <li><a href="#Syntaxe ResultXML">Syntaxe ResultXML</a></li>
            </ul> 
            </li>
            <li><a href="#Function RunConsultationZIP">Function RunConsultationZIP</a>
            <ul>
            <li><a href="#Parametry">Parametry</a></li>                
            </ul> 
            </li>
            <li><a href="#Function RunConsultationZIP2">Function RunConsultationZIP2</a>
            <ul>
            <li><a href="#Parametry">Parametry</a></li>                
            </ul> 
            </li>
        </ul>
        <li><a href="#NestFileClient">NestFileClient</a></li>
        <ul>
         <li><a href="#kcemuprogramslouzi">K èemu program slouží</a></li>
         <li><a href="#soubory">Soubory</a></li>
         <li><a href="#download">Download</a></li>
        </ul>
        </ul>
    
        <a name="dokumentace"></a>
        <h2>
            Dokumentace</h2>
        <a name="Function RunConsultation"></a>
        
        
        <h3>
            Function RunCunsultation(BazeZnalostiXML, AnswersXML) As CosultationResult</h3>
        <a name="Parametry"></a>
        <h4>
            Parametry</h4>
        
        <p>
            BazeZnalostiXML: string obsahující bázi znalostí se syntaxí uvedenou
            níže.</p>
        <p>
            AnswersXML: string obsahující odpovìdi na jednotlivé dotazy báze
            znalostí se syntaxí uvedenou níže.</p>
        <p>
            ConsultationResult: obsahuje 3 promìnné:</p>
        <ul>
            <li>IsOK - boolean promìná udávající, zda konzultace probìhla v poøádku. Pokud ano,
                je platný výsledek v ResultXML, pokud ne, je chybová hláška v errorMessage</li>
            <li>ResultXML - obsahuje výsledek konzultace v syntaxi uvedené níže</li>
            <li>errorMessage - obsahuje chybovou hlášku (neprobìhla-li konzultace správnì)</li>
        </ul>
        <a name="Syntaxe BazeZnalostiXML"></a>
        <h4>
            Syntaxe BazeZnalostiXML</h4>
        <p>
            ...</p>
            <a name="Syntaxe AnswersXML"></a>
        <h4>
            Syntaxe AnswersXML</h4>
        <pre>&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;answers&gt;
  &lt;attribute&gt;
    &lt;id&gt;aaa&lt;/id&gt;
    &lt;type&gt;numeric&lt;/type&gt;
    &lt;answer&gt;
      &lt;value&gt;35,000&lt;/value&gt;
    &lt;/answer&gt;
  &lt;/attribute&gt;
  &lt;attribute&gt;
    &lt;id&gt;bbb&lt;/id&gt;
    &lt;type&gt;binary&lt;/type&gt;
    &lt;answer&gt;
      &lt;weight&gt;1,000&lt;/weight&gt;
    &lt;/answer&gt;
  &lt;/attribute&gt;
  &lt;attribute&gt;
    &lt;id&gt;ccc&lt;/id&gt;
    &lt;type&gt;multiple&lt;/type&gt;
    &lt;answer&gt;
      &lt;value&gt;ca&lt;/value&gt;
      &lt;weight&gt;1,000&lt;/weight&gt;
    &lt;/answer&gt;
    &lt;answer&gt;
      &lt;value&gt;cb&lt;/value&gt;
      &lt;weight&gt;0,000&lt;/weight&gt;
    &lt;/answer&gt;
  &lt;/attribute&gt;
  &lt;attribute&gt;
    &lt;id&gt;ddd&lt;/id&gt;
    &lt;type&gt;single&lt;/type&gt;
    &lt;answer&gt;
      &lt;value&gt;da&lt;/value&gt;
      &lt;weight&gt;1,000&lt;/weight&gt;
    &lt;/answer&gt;
  &lt;/attribute&gt;
  ...
&lt;/answers&gt;
        </pre>
        <p>Pozn. Váhy odpovìdí musí být normovány na interval [-1;1].</p>
        
        <a name="Syntaxe ResultXML"></a>
        
        <h4>
            Syntaxe ResultXML</h4>
        <pre>&lt;?xml version="1.0" encoding="utf-8"?&gt;
&lt;results&gt;
  &lt;goals&gt;
    &lt;attribute&gt;
      &lt;id&gt;aaa&lt;/id&gt;
      &lt;type&gt;binary&lt;/type&gt;
      &lt;answer&gt;
        &lt;weight&gt;0,999&lt;/weight&gt;
      &lt;/answer&gt;
    &lt;/attribute&gt;
  &lt;/goals&gt;
  &lt;questions&gt;
    &lt;attribute&gt;
      &lt;id&gt;bbb&lt;/id&gt;
      &lt;type&gt;numeric&lt;/type&gt;
      &lt;answer&gt;
        &lt;value /&gt;
      &lt;/answer&gt;
    &lt;/attribute&gt;
  &lt;/questions&gt;
&lt;/results>        
        </pre>
        <p>Pozn. Èást &lt;questions&gt; se vyskytuje pouze v pøípadì, kdy pro odvození závìru konzultace
        chybí odpovìï na nìjakou otázku.</p>
        <a name="Function RunConsultationZIP"></a>
        <h3>
            Function RunCunsultationZIP(BazeZnalostiZIP) As CosultationResult</h3>
        <a name="Parametry"></a>
        <h4>
            Parametry</h4>
        
        <p>
            BazeZnalostiZIP: soubor (archiv) ZIP zaslaný do funkce jako pole bytù.
            Archiv musí obsahovat 2 soubory:
            <blockquote>
            BZ.xml - soubor báze znalostí se syntaxí uvedenou výše.
            <br />
            Answers.xml - soubor obsahující odpovìdi na jednotlivé dotazy báze
            znalostí se syntaxí uvedenou výše.</blockquote>
            </p>
        
        <p>
            ConsultationResult: obsahuje 3 promìnné:</p>
        <ul>
            <li>IsOK - boolean promìná udávající, zda konzultace probìhla v poøádku. Pokud ano,
                je platný výsledek v ResultXML, pokud ne, je chybová hláška v errorMessage</li>
            <li>ResultXML - obsahuje výsledek konzultace v syntaxi uvedené níže</li>
            <li>errorMessage - obsahuje chybovou hlášku (neprobìhla-li konzultace správnì)</li>
        </ul>
       
        <a name="Function RunConsultationZIP2"></a>
        <h3>
            Function RunCunsultationZIP2(BazeZnalostiZIP, AnswersXML) As CosultationResult</h3>
        <a name="Parametry"></a>
        <h4>
            Parametry</h4>
        
        <p>
            BazeZnalostiZIP: soubor (archiv) ZIP zaslaný do funkce jako pole bytù.
            Archiv musí obsahovat soubor:
            <blockquote>
            BZ.xml - soubor báze znalostí se syntaxí uvedenou výše.
            </blockquote>
            </p>
        <p>
            AnswersXML: string obsahující odpovìdi na jednotlivé dotazy báze
            znalostí se syntaxí uvedenou níže.</p>
        <p>
            ConsultationResult: obsahuje 3 promìnné:</p>
        <ul>
            <li>IsOK - boolean promìná udávající, zda konzultace probìhla v poøádku. Pokud ano,
                je platný výsledek v ResultXML, pokud ne, je chybová hláška v errorMessage</li>
            <li>ResultXML - obsahuje výsledek konzultace v syntaxi uvedené níže</li>
            <li>errorMessage - obsahuje chybovou hlášku (neprobìhla-li konzultace správnì)</li>
        </ul>
        
        
       
        <a name="NestFileClient"></a>
        <h2>NestFileClient</h2>
        <a name="kcemuprogramslouzi"></a>
        <h3>
            K èemu program slouží</h3>
        <p>
            NestFileClient je program pro Windows sloužící k automatickému volání webové služby
            NESTu. Program naète bázi znalostí a odpovìdi ze souboru, odešle data na zpracování
            webové službe a výsledek opìt uloží do souboru a ihned se sám ukonèí.</p>
            <a name="soubory"></a>
        <h3>
            Soubory</h3>
        <p>Program lze volat se tøemi parametry, pøièemž první udává soubor báze znalostí, druhý
        udává soubor s odpovìïmi a tøetí soubor, do kterého systém uloží odpovìdi.
        Syntaxe souborù
            odpovídá syntaxi pøíslušných parametrù z webové služby.</p>
        <p>
            Pokud je program zavolán bez parametrù, použijí se následující jména souborù (všechny
            musí být umístìny ve stejném adresáøi, jako je samotný program). </p>
        <p>
            BZ.xml - soubor, ze kterého program naèítá bázi znalostí.</p>
        <p>
            answers.xml - soubor, ze kterého program naèítá odpovìdi.</p>
        <p>
            result.xml - soubor, který program vytváøí a ukládá do nìj výsledky.</p>
         <p>Zavolání programu bez parametrù tedy odpovídá následujícímu volání s paramety:<br />
         "NestFileClient.exe BZ.xml answers.xml result.xml"</p>
        <p>
            Další možností je odesílat bázi znalostí zazipovanou pomocí syntaxe:<br />
            "NestFileClient.exe BZ.zip answers.xml result.xml"</p>
        <p>
            Pokud v zip archivu máte i soubor answers.xml, pak použijte syntaxi:<br />
            "NestFileClient.exe BZ.zip" - v tomto pøípadì si ovšem nelze vybrat výstupní soubor.</p>
         <a name="download"></a>
        <h3>
            Download</h3>
         <p><a href="files/NestFileClient.exe">NestFileClient.exe</a> (verze 1.1.0)</p>
    </form>
</body>
</html>
