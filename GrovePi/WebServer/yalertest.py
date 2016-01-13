from flask import Flask, render_template, request
app = Flask(__name__)

@app.route("/")
def main():
    print "Hello route taken (/)"
    return "Hello again from FLASK"
#   return render_template('wlmain.html', **templateData) 

@app.route("/<action>" , methods=['GET', 'POST']) 
def action(action):
    print "Hello action taken (/%s)"%(action )
    if action == "on": 
        message = "Turned " + " on." 
    elif action == "off":
        message = "Turned " +  " off."
    elif action == "toggle":
        message = "Toggled "
    else:
        message = "undefined!"
    return  "Hello action called (/%s) resulting in %s"%(action,message )
    

#   return render_template('wlmain.html', **templateData)

if __name__ == "__main__":
   app.run(host='0.0.0.0', port=80, debug=True)