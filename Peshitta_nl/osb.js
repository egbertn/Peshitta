function processLinks() {
    if (!document.getElementsByTagName) return;
    var anchors = document.getElementsByTagName("a");
    var alen = anchors.length;
    var i = 0;
    var selectedChapter = QueryString('ch');
    for (; i < alen; i++) {
        var anchor = anchors[i];
        if (anchor.href &&  anchor.getAttribute("rel") == "external") {
            anchor.target = "_blank";
            continue;
        }        
        //supports copy-paste text to keyboard to include the real bible reference

        switch (anchor.className) {
            case "seo_1":
                {
                    anchor.style.display = "none";
                    //var t = anchor.innerHTML;
                    //if (t.substring(0, 6) == 'Bijbel') {
                    //    t = t.substr(6);
                    //}
                    //var p = t.indexOf(':');
                    //if (p > 0)
                    //    anchor.innerHTML = t.substring(0, p + 1);
                }
                break;
            case "amen":
                {
                    var chapter = anchor.innerHTML;
                    var findGt = chapter.lastIndexOf('>');
                    var text = findGt > 0 ? chapter.substr(findGt + 1) : chapter;
                    if (text != null && text.length > 0) {
                        var iText = parseInt(text);
                        if (iText != NaN && iText == selectedChapter && iText.toString().length == text.length) {
                            //anchor.style.background = "#CC9900";
                            anchor.className += " selectedverse";
                        }
                    }
                }
                break;
        }
    }
    if (document.getElementsByClassName) {
        var els = document.getElementsByClassName("seo_1");
        alen = els.length;
        for (i = 0; i < alen; i++) {
            if (els[i].tagName == 'TD') {
                els[i].style.display = 'none';
            }
        }
        var spans = document.getElementsByClassName("seo_2");
        alen = spans.length;
        for (i = 0; i < alen; i++) {
        var span = spans[i];        
        span.style.display = "none"; 
        }
    }
    
}



/*copyright ADC Cure*/
function body_onload()
{
    var par = QueryString('goto');
    if (par != null && par.length > 0) {
            //patch
        if (par.charAt(0) != 'p')
            par = 'p' + par;
 
        var vers = document.getElementById(par);
        if (vers != null) {
            var spans = vers.getElementsByTagName("span");
            for (var x = 0; x < spans.length; x++) {
                var sp = spans.item(x);
                if (sp.className == 'verse') {
                    sp.style.backgroundColor = 'yellow';
                }
            }
            vers.scrollIntoView(true);
        }
    }
    processLinks();
}
function jumpv(v, e) {

    if (v != null && v.length > 0) {
        //patch
        if (v.charAt(0) != 'p')
         v = 'p' + v;        
    }
    var verse = document.getElementById(v);
    
    if (verse == null) {
        alert('cannot activate verse');

    }
    else {
        verse.scrollIntoView(true);
        if (e) {
            e.cancelBubble = true;
        }
    }
	return false;
}

function QueryString(idx)
{
    var loc = location.href;
    var qMark = loc.indexOf('?');
    
    if (qMark > 0)
    {
        var pars= loc.substr(qMark + 1);
        pars = pars.split('&');
        var cx=pars.length;
        while(cx-- != 0 )
        {
            var splitted = pars[cx].split('=');
            if (cx == idx || splitted[0] == idx )
            {
                return splitted[1];                
            }
        }
        
    }
}
var osugWindow;
function showv(phref) {
    var width = 750;
    var height = 380;
    var left = parseInt((screen.availWidth / 2) - (width / 2));
    var top = parseInt((screen.availHeight / 2) - (height / 2));
    osugWindow = window.open(phref, 'verse', 'status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=0,scrollbars=0,height=' + height + ',width=' + width + ',left=' + left + ',top=' + top);
}
function suggest(id) {
    var width = 600;
    var height = 500;
    var left = parseInt((screen.availWidth / 2) - (width / 2));
    var top = parseInt((screen.availHeight / 2) - (height / 2));
    osugWindow = window.open('s' + 'u' + 'g' + 'gestion' + '.aspx?t=' + id, 'suggest', 'status=0,toolbar=0,location=0,menubar=0,directories=0,resizable=0,scrollbars=0,height=' + height + ',width=' + width +',left=' + left + ',top=' + top);
}
function closeSuggest() {
    osugWindow.close();
}
function ShowNote(e, obj, start, width) {
    var testit = private_getObj(e, obj);        
    var goAway = testit.attributes.getNamedItem("nomouseover");
    if (goAway != null) {
        
        return false; 
    }
    if (testit != null && testit.style && testit.style.display != 'block') {
        return ShowHideNote(e, obj, start, width);
    }
}
/*toggle */
function FixNote(e, obj) {
    var testit = private_getObj(e, obj);
    var goAway = testit.attributes.getNamedItem("nomouseover");
    if (goAway == null) {
        goAway = document.createAttribute("nomouseover");
        goAway.value = "1";
        testit.attributes.setNamedItem(goAway);
    }
    else {
        testit.attributes.removeNamedItem("nomouseover");
    }
}
function HideNote(e, obj) {
    var testit = private_getObj(e, obj);
    var goAway = testit.attributes.getNamedItem("nomouseover");    
    if (goAway != null) {

        return false;
    }
    if (testit != null && testit.style && testit.style.display == 'block') {
        testit.style.display = 'none';
    }
}

function GetHeight() {

    if (self.innerHeight) {
        return self.innerHeight;
    }
    else if (document.documentElement && document.documentElement.clientHeight) {
        return document.documentElement.clientHeight;
    }
    else if (document.body) {
        return document.body.clientHeight;
    }
    else {
        return window.screen.Height / 2;
    }

}

function private_getObj(e, obj) {
    var ev = (!e) ? window.event : e; //IE:Moz
    var src = e.srcElement == null ? e.target : e.srcElement;
    if (src == null) {
        alert('sorry, your browser fails to work');
        return false;
    }    
    var testit = src.parentNode; //find outer P which is a verse marker
    while (testit.nodeName != 'TD') {
        testit = testit.parentNode;
    }
    testit = testit.getElementsByClassName('noteText'); //the verse itself.
    var note = testit[obj];
    if (note == null) {
        alert('no footnotes found');
        return null;
    }
    return note;
}
  // mouse x-y pos.s
  var tempX, tempY;
function ShowHideNote (e,obj,start,width) {

	tempX = 0, tempY = 0;
	var ev = (!e) ? window.event : e; //IE:Moz
	if (ev.pageX){ // event by Mozilla or Opera
		tempX = ev.pageX;
		tempY = ev.pageY;
	} else if (ev.clientX){ // event by Explorer 6
		tempX = ev.clientX+document.body.scrollLeft;
		tempY = ev.clientY+document.body.scrollTop;
		// compliance mode for old exploror 5
	    tempY += document.documentElement ? document.documentElement.scrollTop : document.body.scrollTop;
	} else { // old browsers
		tempX = 0
		tempY = 0
	}
	// reset values if incorrect
	tempX += document.body.scrollLeft;
	if (tempX < 0) tempX = 0;
	if (tempY < 0) tempY = 0;	
	// reset variables
	if (width == undefined) width = 300;
	if (start == undefined) start = tempX;

	var testit = private_getObj(e, obj);	
	var st = testit.style;
	st.position = 'absolute';
	var isshown = st.display == 'block';
	st.display = isshown  ? 'none' : 'block';
	st.top = tempY +'px';
	st.left = start + 'px';
	st.width = width + 'px';
	return true;
}