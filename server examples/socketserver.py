import socket
import sys
import random
import time
import math

# Create a TCP/IP socket

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 5111)
print >>sys.stderr, 'starting up on %s port %s' % server_address
sock.bind(server_address)
# Listen for incoming connections
sock.listen(1)

while True:
    # Wait for a connection
    print >>sys.stderr, 'waiting for a connection'
    connection, client_address = sock.accept()
    try:
        print >>sys.stderr, 'connection from', client_address

        # Receive the data in small chunks and retransmit it
        while True:
            data = connection.recv(16)
            print >>sys.stderr, 'received "%s"' % data
            if data:
                print >>sys.stderr, 'sending data back to the client'
            else:
                print >>sys.stderr, 'no more data from', client_address
                break
            #connection.sendall('C,0,0,0,1\r\n')
            #generate a egg carton mesh        
            Nx=50
            Nz=50
            dNx=2.
            dNz=2.
            t=0
            message="M,%d,%d,%g,%g" % (Nx,Nz,dNx,dNz)
            for z in range (0,Nz):
                for x in range (0,Nx):
                    #y=0;
                    y=10.*math.sin(float(x)/float(Nx)*2.*math.pi*3.+float(t)/10)*math.cos(float(z)/float(Nz)*2.*math.pi*3.+float(t)/10)
                    message=message+",%g" % (y)
            connection.sendall(message+"\r\n")    
            mesh_no = connection.recv(16)
            print >>sys.stderr, 'Object Number Mesh: "%s"' % (mesh_no)
            #translate it to the center 
            connection.sendall("t,p,%d,%g,%g,%g\r\n" % (int(mesh_no),-float(Nx)*dNx/2.,0.,-float(Nz)*dNz/2.))
            #add a resource (simple coordinate system)
            connection.sendall("L,simple_coordinates\r\n")
            data = connection.recv(16)
            #animate the mesh data
            t=0
            while True:
                message="K"
                for i in range (0,100):
                    x=20.*math.pow(1.+math.sin(float(t)/10),2.)*math.cos(float(i)*2.*math.pi/100.)
                    z=20.*math.pow(1.+math.sin(float(t)/10),2.)*math.sin(float(i)*2.*math.pi/100.)
                    y=20.*math.sin(float(t)/100.)
                    message=message+",%g,%g,%g" %(x,y,z)
                connection.sendall(message+"\r\n")
                curve_no=connection.recv(16)
                print >>sys.stderr, 'Object Number Curve: "%s"' %data
                t=t+1
                message="m,%d" % int(mesh_no)
                for z in range (0,Nz):
                    for x in range (0,Nx):
                        y=10.*math.sin(float(x)/float(Nx)*2.*math.pi*3.+float(t)/10)*math.cos(float(z)/float(Nz)*2.*math.pi*3.+float(t)/10)
                        message=message+",%g" % (y)
                connection.sendall(message+"\r\n")
                connection.sendall("D,%s\r\n" % (curve_no))
                #time.sleep(.05)
                
             
            
            
            # maxt=50
            # for t in range (0,maxt):
            #     message="m,%d" % int(data)
            #     for z in range (0,Nz):
            #         for x in range (0,Nx):
            #             #y=0;
            #             y=10.*math.sin(float(x)/float(Nx)*2.*math.pi*3.+float(t)/10)*math.cos(float(z)/float(Nz)*2.*math.pi*3.+float(t)/10)
            #             message=message+",%g" % (y)
            #     connection.sendall(message+"\r\n")
            #     connection.sendall("t,p,%d,0,%g,0\r\n" % (int(data),float(t)/maxt*10))
            #     connection.sendall("t,s,%d,%g,%g,%g\r\n" % (int(data),1.+3.*float(t)/maxt,1.+3.*float(t)/maxt,1.+3.*float(t)/maxt))
            #     connection.sendall("t,r,%d,%g,0,0\r\n" % (int(data),float(t)/maxt*90))
                
            #     time.sleep(.01)             
                


            #Spiral Linear Mesh
            # for t in range (0,10000): 
            #     message="K"
            #     for i in range (1,300):
            #         x=(3+2*math.sin(t/10.))*math.sin(i/10.+t/10.)
            #         y=5.0*math.cos(i/10.+t/10.)
            #         z=(2.+math.cos(t/10.))+i/10.;
            #         message=message+",%g,%g,%g" % (x,y,z)
            #     connection.sendall(message+"\r\n")
            #     data = connection.recv(16)
            #     print >>sys.stderr, 'Object Number: "%s"' %data
            #     time.sleep(.01)
            #     connection.sendall("D,%d\r\n" % (t))
                      
            #connection.sendall('B,0,0,0,1,0,0,2,0,0,5,1,0,10,4,0\r\n')
            # for i in range(1,100):
            #     connection.sendall("C,%g,%g,%g,%g\r\n" % (random.uniform(-10,10),random.uniform(-10,10),random.uniform(-10,10),random.uniform(0.1,5)))            
            #     time.sleep(.3)
             
            
    finally:
        # Clean up the connection
        connection.close()