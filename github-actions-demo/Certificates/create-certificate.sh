#!/bin/bash

# How to use:
# -n flag is the fully qualified name (FQDN) (eg  ots.la.gov). This is required.
# -p flag is for the certificate passphrase (eg 1234). This is required.
# To run the script "./create-certificate.sh -n ots.la.gov -p 1234 -e pd.ots@la.gov [OPTIONS]"
# -c flag to create optional CA root
# example to run the script "./create-certificate.sh -n ots.la.gov -p 1234 -e pd.ots@la.gov -c"
# ./create-certificate.sh -h to view all the options

KEYSIZE=2048
DAYS=3650
EMAIL=opensource@la.gov
CA=false
PEM=false
VIEW=false
VERIFY=false
OUTPUT=.

Help()
{
   # Display Help
   echo "*************************************************************************************************"
   echo "This script 'create-certificate.sh' can be used to generate self signed PFX, CRT and PEM certificates including CA ROOT CERT if option '-c' is included"
   echo   
   echo "Syntax: scriptTemplate -n NAME -p PASSPHRASE"
   echo  
   echo "options:"
   echo "-n     NAME:Fully qualified server (or service) name (required)."
   echo "-p     PASSPHRASE:Passphrase for certificate (required)."
   echo "-c     CA ROOT CERT:Create a CA root certificate (optional)."
   echo "-P     PEM FORMAT: Add PEM format certificates (optional)."
   echo "-o     OUTPUT: Set a subdirectory path." 
   echo "-k     KEYSIZE:Enter keysize, default is 2048 (optional)."
   echo "-d     DAYS:Enter number of days until expiration, default is 3650 (optional)."
   echo "-e     EMAIL:The email associated with the certificate, default is opensource@la.gov (optional)."  
   echo "-v     VIEW:View certificates (optional)."
   echo "-V     VERIFY:Verify private keys and if it matches certificate and CSR (optional)."
   echo "-h     Help."
   echo "*************************************************************************************************"
}

# Get the options
while getopts ":hn:p:k:d:e:cvVPo:" option; do
   case $option in
      h) # display Help
         Help
         exit;;
      n) # Enter a name
         FQDN="$OPTARG";;
      p) # Enter a passphrase
         PASSPHRASE="$OPTARG";;
      c) # Add ca if root CA cert needs to be created 
         CA="true";;
      P) # PEM format included 
         PEM="true";;
      o) # Output subdirectory path
         OUTPUT="$OPTARG";;
      k) # Enter keysize, default is 2048
	 KEYSIZE="$OPTARG";;
      d) # Enter number of days, default is 3650
	 DAYS="$OPTARG";;
      e) # Enter the email, default is someone@ots.la.gov
         EMAIL="$OPTARG";;
      v) # View certificates in console, default is false
	 VIEW="true";;
      V) # Verify private key matches certificate and CSR, default is false
	 VERIFY="true";;
     \?) # Invalid option
         echo "Invalid option -$OPTARG" >&2
         echo "Run 'create-certificate.sh -h' to view all the options"   
         echo         
         Help      
         exit;;
   esac
done

# [optional] Remove all options processed by getopts.
shift $(( OPTIND - 1 ))
[[ "${1}" == "--" ]] && shift

if [ -z "$FQDN" ] & [ -z "$PASSPHRASE" ]
then
  echo "Error: No FQDN and passphrase arguments provided"
  echo "Usage: Provide FQDN and passphrase as arguments"
  echo "Help: use -h to provide options"
  exit 1
fi

if [ -z "$FQDN" ]
then
  echo "Error: No FQDN argument provided"
  echo "Usage: Provide a FQDN as an argument"
  exit 1
fi

if [ -z "$PASSPHRASE" ]
then
  echo "Error: No passphrase argument provided"
  echo "Usage: Provide a passphrase as an argument"
  exit 1
fi

if [ $(expr length "$PASSPHRASE") -lt 4 ]
then
  echo "Error: Passphrase argument must be greater than 4 characters."
  echo "Usage: Provide a passphrase as an argument"
  exit 1
fi

if [ "$KEYSIZE" != 2048 ] && [ "$KEYSIZE" != 4096 ] && [ "$KEYSIZE" != 3072 ]
then
  echo "Error: Keysize value for RSA must be equal to 2048 or 3072 or 4096."
  echo "Usage: Provide a keysize value as an argument"
  exit 1
fi

 if [ -d "$OUTPUT" ]; then
   echo "Directory exists."
 else
  mkdir --parents "$OUTPUT"
fi

echo "Fully Qualified Name of Service: ${FQDN}"
echo "Passphrase: ${PASSPHRASE}"
echo "Keysize: ${KEYSIZE}"
echo "Days: ${DAYS}"
echo "Email: ${EMAIL}"
echo "CA: ${CA}"
echo "PEM: ${PEM}"
echo "Output Path: ${OUTPUT}" 
echo "View Certificate: ${VIEW}"
echo "Verify Certificate: ${VERIFY}"

if ${CA};
then
echo "Generating common root certificate as CRT"
openssl req -x509 \
  -sha256 -days ${DAYS} \
  -nodes \
  -newkey rsa:${KEYSIZE} \
  -subj "/CN=${i}/C=US/ST=Louisiana/L=Baton Rouge/O=Office of Technology Services/OU=Product Delivery" \
  -keyout ${OUTPUT}/root.key -out ${OUTPUT}/root.crt \
  -passin pass:${PASSPHRASE}

 if ${PEM};
 then 
 echo "Generating common root certificate as PEM"
 openssl req -x509 \
  -sha256 -days ${DAYS} \
  -nodes \
  -newkey rsa:${KEYSIZE} \
  -subj "/CN=${i}/C=US/ST=Louisiana/L=Baton Rouge/O=Office of Technology Services/OU=Product Delivery" \
  -keyout ${OUTPUT}/root-private-key.pem -out ${OUTPUT}/root-cert.pem \
  -passin pass:${PASSPHRASE}
 fi
fi

echo "Generating an RSA private key"
openssl genrsa -passout pass:${PASSPHRASE} -out ${OUTPUT}/${FQDN}.key ${KEYSIZE}

 if ${PEM};
 then 
 echo "Generating a PEM format RSA private key"
 openssl genrsa -passout pass:${PASSPHRASE} -out ${OUTPUT}/${FQDN}-private-key.pem ${KEYSIZE}
 fi


if ${VERIFY};
then
echo "Check that a private key ${FQDN}.key is a valid key"
openssl rsa -check -in ${OUTPUT}/${FQDN}.key

 if ${PEM};
 then 
 echo "Check that a private key ${FQDN}.key PEM format is a valid key"
 openssl rsa -check -in ${OUTPUT}/${FQDN}-private-key.pem
 fi
fi

if ${PEM};
 then 
 echo "Generating a PEM RSA public key from private key"
 openssl rsa -in ${OUTPUT}/${FQDN}-private-key.pem -pubout -out ${OUTPUT}/${FQDN}-public-key.pem
fi

echo "Creating CSR config"
  cat > ${OUTPUT}/${FQDN}.csr.conf <<EOF

[ req ]
default_bits = ${KEYSIZE}
prompt = no
default_md = sha256
req_extensions = req_ext
distinguished_name = dn
x509_extensions = v3_ca

[ dn ]
C = US
ST = Louisiana
L = Baton Rouge
O = Office of Technology Services
OU = Product Delivery
CN = ${FQDN}
emailAddress = ${EMAIL}

[ req_ext ]
subjectAltName = @alt_names

[ v3_ca ]
subjectAltName = @alt_names

[ alt_names ]
DNS.1 = ${FQDN}

EOF

if ${VIEW};
then 
echo "View CSR entries"
 openssl req -text -noout -verify -in ${OUTPUT}/${FQDN}.csr
fi

echo "Creating CSR request using private key"
openssl req -key ${OUTPUT}/${FQDN}.key -passin pass:${PASSPHRASE} -new -config ${OUTPUT}/${FQDN}.csr.conf -out ${OUTPUT}/${FQDN}.csr

 echo "Creating certificate config"
  cat > ${OUTPUT}/${FQDN}.cert.conf <<EOF

basicConstraints = CA:FALSE
authorityKeyIdentifier = keyid,issuer
keyUsage = critical, nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = ${FQDN}

EOF

if ${CA};
then
echo "Creating certificate with self signed CA"
  openssl x509 -CAkey ${OUTPUT}/root.key -in ${OUTPUT}/${FQDN}.csr -passin pass:${PASSPHRASE} -req  -days ${DAYS} -out ${OUTPUT}/${FQDN}.crt \
    -CA ${OUTPUT}/root.crt  \
    -CAcreateserial \
    -sha256 \
    -extfile ${OUTPUT}/${FQDN}.cert.conf

if ${PEM};
then 
echo "Creating PEM certificate with self signed CA"
openssl x509 -CAkey ${OUTPUT}/root.key -in ${OUTPUT}/${FQDN}.csr -passin pass:${PASSPHRASE} -req -days ${DAYS} -out ${OUTPUT}/${FQDN}-cert.pem \
    -CA ${OUTPUT}/root.crt  \
    -CAcreateserial \
    -sha256 \
    -extfile ${OUTPUT}/${FQDN}.cert.conf
fi

if ${VERIFY};
 then
  echo "Verify that a certificate ${FQDN}.crt was signed by a root CA certificate"
  openssl verify -verbose -CAfile ${OUTPUT}/root.crt ${OUTPUT}/${FQDN}.crt
fi
else
echo "Creating a self-signed certificate as CRT and PEM formats"
openssl x509 -signkey ${OUTPUT}/${FQDN}.key -in ${OUTPUT}/${FQDN}.csr -passin pass:${PASSPHRASE} -req -days ${DAYS} -out ${OUTPUT}/${FQDN}.crt

 if ${PEM};
 then 
 openssl x509 -signkey ${OUTPUT}/${FQDN}-private-key.pem -in ${OUTPUT}/${FQDN}.csr -passin pass:${PASSPHRASE} -req -days ${DAYS} -out ${OUTPUT}/${FQDN}-cert.pem
 fi
fi

if ${VIEW};
 then
 echo "View the contents of a certificate ${FQDN}.crt"
 openssl x509 -text -noout -in ${OUTPUT}/${FQDN}.crt
fi

if ${VERIFY};
 then
 echo "Verify if a private key ${FQDN}.key matches a certificate ${FQDN}.crt and CSR ${FQDN}.csr"
 echo "If the output of each command is identical there is an extremely high probability that the private key, certificate, and CSR are related"
 openssl rsa -noout -modulus -in ${OUTPUT}/${FQDN}.key | openssl md5
 openssl x509 -noout -modulus -in ${OUTPUT}/${FQDN}.crt | openssl md5
 openssl req -noout -modulus -in ${OUTPUT}/${FQDN}.csr | openssl md5
fi

if ${CA};
then
echo "Converting CRT to PFX with CA"
openssl pkcs12 -inkey ${OUTPUT}/${FQDN}.key -in ${OUTPUT}/${FQDN}.crt -passin pass:${PASSPHRASE} -export -out ${OUTPUT}/${FQDN}.pfx -certfile ${OUTPUT}/root.crt -passout pass:${PASSPHRASE}
else
echo "Converting CRT to PFX"
openssl pkcs12 -inkey ${OUTPUT}/${FQDN}.key -in ${OUTPUT}/${FQDN}.crt -passin pass:${PASSPHRASE} -export -out ${OUTPUT}/${FQDN}.pfx -passout pass:${PASSPHRASE}
fi

 if ${PEM};
 then 
 echo "Creating combined PEM from PFX"
 echo "If your PFX file has multiple items in it (e.g. a certificate and private key), the PEM file that is created will contain all of the items in it."
 openssl pkcs12 -in ${OUTPUT}/${FQDN}.pfx -passin pass:${PASSPHRASE} -nodes -out ${OUTPUT}/${FQDN}.combined.pem
 fi

echo "Cleanup"
rm ${OUTPUT}/${FQDN}.csr
rm ${OUTPUT}/${FQDN}.csr.conf
rm ${OUTPUT}/${FQDN}.cert.conf

if ${CA};
then
rm ${OUTPUT}/root.srl
fi



