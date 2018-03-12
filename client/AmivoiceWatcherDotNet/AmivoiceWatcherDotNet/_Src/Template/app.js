(function () {
    var app = angular.module('myApp', ['ngSanitize']);

    app.controller('myController', ['$scope', '$window', function ($scope, $window) {
        $scope.msg = $window.msg;
        $scope.path = $window.path;
        $scope.title = $window.msg["title"];;
    }]).directive('msg', function () {

        //htmlReturn =  'Name: {{msgObj["title"]}}<br /> Address: {{msgObj["links"]}}'

        //var $msg = $scope.msgObj;
        //var msgO =  $scope.msgObj;

        //imgHtml
        imgHtml = ""

        if ('icon' in msg) {
            imgHtml = '<img id="left_image" alt="star" ng-src="{{msg["icon"]}}" />'


        } else {
            if ('level' in msg) {
                if (msg["level"] == "notice") {
                    msg["icon"] = "Button-Info-icon.png"

                } else if (msg["level"] == "error") {
                    msg["icon"] = "Button-Close-icon.png"

                } else if (msg["level"] == "notice") {
                    msg["icon"] = "Button-Info-icon.png"

                } else if (msg["level"] == "warning") {
                    msg["icon"] = "Button-Warning-icon.png"

                } else if (msg["level"] == "question") {
                    msg["icon"] = "Help-icon.png"

                } else if (msg["level"] == "success") {
                    msg["icon"] = "ok-icon.png"

                }



                imgHtml = '<img id="left_image" alt=\'{{path}}/_Src/Template/image/{{msg["icon"]}}\' ng-src=\'{{path}}/_Src/Template/image/{{msg["icon"]}}\' />'
            } else {
                imgHtml = "";
            }




        }



        //define message body = msgbody
        msgbody = '';

        if ("links" in msg) {
            var links = msg["links"];;
            //alert(links)
            if (links.indexOf("[links]") == -1) {

                splitLink = links.split("&&")
                splitLinkLength = splitLink.length
                //var body = document.getElementById("messageBody");
                for (i = 0; i < splitLinkLength; i++) {

                    var splitsplitLink = splitLink[i].split(">")
                    //var linkElement = document.createElement("a");
                    //linkElement.setAttribute("id", "link" + i);

                    msgbody2Add = '<a id="link' + i + '" ';




                    href = splitsplitLink[1]
                    if (href.toLowerCase().indexOf("http://") == -1) {
                        href = "http://" + href
                    }

                    msgbody2Add += 'href="javascript:window.external.openLinkInDefaultBrowser(\'' + href + '\')">' + splitsplitLink[0] + '</a>';




                    //linkElement.setAttribute("href", "javascript:window.external.openLinkInDefaultBrowser('" + href + "')");
                    //linkElement.innerHTML = splitsplitLink[0];
                    //buttonTemp.className = "answerButton"


                    // 2. Append somewhere
                    //var body = document.getElementById("messageBody");
                    //var messageBody = document.getElementById("messageBody");
                    //messageBody.innerHTML += "";
                    //messageBody.appendChild(linkElement);
                    //messageBody.innerHTML += "<br />";

                    msgbody += msgbody2Add;
                    msgbody += "<br />"

                }



            }
        }


        html2return = '';


        //http://www.w3schools.com/cssref/w3schools_logo.gif
        // <img id="left_image" alt="star" ng-src="{{msg['image']}}" />

        //$scope.myHTML =
        //$scope.titleX = msg["title"];

        //$scope.titleX ="<i>xxxx</i>";
        //<span ng-bind-html-unsafe="title">Hello {{title}}!</span>\

        html2return = '\
<div class="customDiv">\
<br />\
<div class="message">\
<p  id="title" class="title">\
{{imgHtml}}<span ng-bind-html="title"></span>\
</p>\
<p id="messageBody">\
{{msgbody}}\
</p>\
<p id="datetime" class="date">sent on {{msg["datetime"]}}</p></div>\
</div>\
';

        //html2return+='       {{msg["body"]}}';
        //html2return+=msgbody;
        html2return = html2return.replace("{{msgbody}}", msgbody)
        html2return = html2return.replace("{{imgHtml}}", imgHtml)



        //html2return=$sce.trustAsHtml(html2return);

        return {
            template: html2return
        };
    });



    //app.directive('msg', function() {
    //	var msg={"id":"FmanunsG","title":"Found results for","body":"","level":"notice","datetime":"2017-01-26 15:56:35","links":"Amivoice>http://www.advanced-media.co.jp/&&XXX>YYY.com&&XXX2>YYY.com&&XXX3>YYY.com&&XXX4>YYY.com&&XXX5>YYY.com&&XXX6>YYY.com","duration":-1};
    //
    //	
    //	return {
    //	    template: 'Name:,AAAA, {{msg["title"]}}'+msg["title"]
    //	};
    //});



})();
