# cheap asp.net 
from grovepi import *
from flask import Flask, render_template, request
app = Flask(__name__)

# connect the BLUE  led to D3
# connect the GREEN led to D4


pins = {
    3 : {'name' : 'BLUE coffee maker', 'state' : 0},
    4 : {'name' : 'GREEN lamp', 'state' : 0}
    } 

for pin in pins:
    pinMode(pin,"OUTPUT")

@app.route("/")
def main():
    for pin in pins:
        pins[pin]['state'] = digitalRead(pin) 
    templateData = {
        'pins' : pins 
        }
    return render_template('wlmain.html', **templateData) 

@app.route("/<changePin>/<action>" , methods=['GET', 'POST']) 
def action(changePin, action):
    changePin = int(changePin) 
    deviceName = pins[changePin]['name'] 
    if action == "on": 
        digitalWrite(changePin, 1) 
        message = "Turned " + deviceName + " on." 
    if action == "off":
        digitalWrite(changePin, 0)
        message = "Turned " + deviceName + " off."
    if action == "toggle":
        digitalWrite(changePin, not digitalRead(changePin)) 
        message = "Toggled " + deviceName + "."

    for pin in pins:
        pins[pin]['state'] = digitalRead(pin) 

    templateData = {
        'message' : message,
        'pins' : pins
    } 

    return render_template('wlmain.html', **templateData)

if __name__ == "__main__":
    app.run(host='0.0.0.0', port=80, debug=True)