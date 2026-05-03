import hmac, hashlib, time, json
from urllib.parse import quote

# === НАСТРОЙКИ ===
BOT_TOKEN = "8729935629:AAHzULxixU97vJDEg423IpxpzeqTEojt5LU"
TELEGRAM_USER_ID = 2085987866  # любое число
USERNAME = "timurmuhammadxon"
FIRST_NAME = "Timur"
LAST_NAME = "Muhammadxon"
# =================

user_obj = {
    "id": TELEGRAM_USER_ID,
    "username": USERNAME,
    "first_name": FIRST_NAME,
    "last_name": LAST_NAME,
}
user_json = json.dumps(user_obj, separators=(',', ':'))
auth_date = int(time.time())

# Параметры в alphabetical order
params = {
    "auth_date": str(auth_date),
    "user": user_json,
}

# data-check-string — отсортированные пары через \n, БЕЗ url-encoding
data_check_string = "\n".join(f"{k}={v}" for k, v in sorted(params.items()))

# secret_key = HMAC_SHA256("WebAppData", bot_token)
secret_key = hmac.new(b"WebAppData", BOT_TOKEN.encode(), hashlib.sha256).digest()

# hash = HMAC_SHA256(secret_key, data_check_string)
computed_hash = hmac.new(secret_key, data_check_string.encode(), hashlib.sha256).hexdigest()

# initData — это URL-encoded query string
init_data = "&".join(f"{k}={quote(v, safe='')}" for k, v in params.items())
init_data += f"&hash={computed_hash}"

print("initData:")
print(init_data)
print()
print("JSON для Swagger:")
print(json.dumps({"initData": init_data}, indent=2))