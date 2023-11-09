# init:
# 	docker compose up   -d db
# 	docker compose exec -T db psql -U tzkt postgres -c '\l'
# 	docker compose exec -T db dropdb -U tzkt --if-exists tzkt_db
# 	docker compose exec -T db createdb -U tzkt -T template0 tzkt_db
# 	docker compose exec -T db apt update
# 	docker compose exec -T db apt install -y wget
# 	docker compose exec -T db wget "https://snapshots.tzkt.io/tzkt_v1.12_mainnet.backup" -O tzkt_db.backup
# 	docker compose exec -T db pg_restore -U tzkt -O -x -v -d tzkt_db -e -j 4 tzkt_db.backup
# 	docker compose exec -T db rm tzkt_db.backup
# 	docker compose exec -T db apt autoremove --purge -y wget
# 	docker compose pull	

init:
	docker compose up   -d db
	docker compose exec -T db psql -U tzkt postgres -c '\l'
	docker compose exec -T db dropdb -U tzkt --if-exists tzkt_db
	docker compose exec -T db createdb -U tzkt -T template0 tzkt_db
	docker compose exec -T db apt update
	docker compose exec -T db apt install -y wget
	docker compose exec -T db apt autoremove --purge -y wget
	docker compose pull	

start:
	docker compose up -d

stop:
	docker compose down

update:
	git pull
	docker compose build

clean:
	docker system prune --force

db-start:
	docker compose up -d db

migration:
	# Install EF: dotnet tool install --global dotnet-ef
	export $$(cat .env | xargs) && cd Tzkt.Data && dotnet-ef database update -s ../Tzkt.Sync/Tzkt.Sync.csproj

sync:
	# Set up env file: cp .env.sample .env
	export $$(cat .env | xargs) && dotnet run -p Tzkt.Sync -v normal

api:
	# Set up env file: cp .env.sample .env
	export $$(cat .env | xargs) && dotnet run -p Tzkt.Api -v normal

api-image:
	docker build -t mavrykdynamics/tzkt-api:1.12.4 -f ./Tzkt.Api/Dockerfile .

sync-image:
	docker build -t mavrykdynamics/tzkt-sync:1.12.4 -f ./Tzkt.Sync/Dockerfile .

basenet-init:
	docker compose -f docker-compose.basenet.yml up   -d basenet-db
	docker compose -f docker-compose.basenet.yml exec -T basenet-db psql -U tzkt postgres -c '\l'
	docker compose -f docker-compose.basenet.yml exec -T basenet-db dropdb -U tzkt --if-exists tzkt_db
	docker compose -f docker-compose.basenet.yml exec -T basenet-db createdb -U tzkt -T template0 tzkt_db
	docker compose -f docker-compose.basenet.yml exec -T basenet-db apt update
	docker compose -f docker-compose.basenet.yml exec -T basenet-db apt install -y wget
	docker compose -f docker-compose.basenet.yml exec -T basenet-db apt autoremove --purge -y wget
	docker compose pull	
	
basenet-start:
	docker compose -f docker-compose.basenet.yml up -d

basenet-stop:
	docker compose -f docker-compose.basenet.yml down

basenet-db-start:
	docker compose -f docker-compose.basenet.yml up -d basenet-db
reset:
	docker compose -f docker-compose.basenet.yml down --volumes
	docker compose -f docker-compose.basenet.yml up -d basenet-db