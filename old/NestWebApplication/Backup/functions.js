// JScript File
function Zmiz(divname){
       div = document.getElementById('d'+divname);
       if (div.style.visibility == "hidden"){
        div.style.visibility = "visible";
        div.style.position = "";
        document.getElementById("prepinac"+divname).innerHTML = "–";
       }
       else {
        div.style.visibility = "hidden";
        div.style.position = "absolute";
        document.getElementById("prepinac"+divname).innerHTML = "+";
       }; 
       };
       function Ukaz(divname){
         div = document.getElementById('d'+divname);
         div.style.visibility = "visible";
         div.style.position = "";
         document.getElementById("prepinac"+divname).innerHTML = "–";
       }  
       function Skryj(divname){
         div = document.getElementById('d'+divname);
         div.style.visibility = "hidden";
         div.style.position = "absolute";
         document.getElementById("prepinac"+divname).innerHTML = "+";
       }  
