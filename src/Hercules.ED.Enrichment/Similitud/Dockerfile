FROM ubuntu:20.04

RUN apt update && DEBIAN_FRONTEND=noninteractive apt -y install python3 python3-pip apache2 libapache2-mod-wsgi-py3

WORKDIR /hercules-similarity-api

COPY requirements.txt .
RUN pip install -r requirements.txt

COPY . .

COPY docker_resources/similarity_api.conf /etc/apache2/sites-available/
COPY docker_resources/similarity_api.wsgi /hercules-similarity-api/
RUN mkdir /var/log/hercules_similarity

RUN a2ensite similarity_api.conf

RUN mkdir /var/www/.cache
RUN chgrp www-data /var/www/.cache
RUN chmod g+w /var/www/.cache

EXPOSE 80
CMD ["apachectl", "-D", "FOREGROUND"]
