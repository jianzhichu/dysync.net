# 错误即终止
$ErrorActionPreference = "Stop"

Write-Host "===== 1/3 开始构建 x86 beta 版本 =====" -ForegroundColor Cyan
docker build -t jianzhichu/dysync.net:beta_2.2.3 .
docker tag jianzhichu/dysync.net:beta_2.2.3 ccr.ccs.tencentyun.com/jianzhichu/dysync:beta_2.2.3
docker push ccr.ccs.tencentyun.com/jianzhichu/dysync:beta_2.2.3
Write-Host "x86 beta 版本推送完成" -ForegroundColor Green

try {
    Write-Host "===== 2/3 切换 ARM 配置文件并构建 =====" -ForegroundColor Cyan
    # 备份原配置，切换为 ARM 配置
    Rename-Item appsettings.json appsettings.json.bak
    Rename-Item appsettings-arm.json appsettings.json

    # 指定 Dockerfile-arm 构建，无需重命名 Dockerfile
    docker buildx build --platform linux/arm64 -f Dockerfile-arm -t jianzhichu/dysync.net:arm_2.2.3 .
    docker tag jianzhichu/dysync.net:arm_2.2.3 ccr.ccs.tencentyun.com/jianzhichu/dysync:arm_2.2.3
    docker push ccr.ccs.tencentyun.com/jianzhichu/dysync:arm_2.2.3
    Write-Host "ARM 版本推送完成" -ForegroundColor Green
}
finally {
    # 无论成功失败，都恢复原配置文件
    Write-Host "===== 3/3 恢复原配置文件 =====" -ForegroundColor Cyan
    if (Test-Path appsettings.json) {
        Rename-Item appsettings.json appsettings-arm.json -Force
    }
    if (Test-Path appsettings.json.bak) {
        Rename-Item appsettings.json.bak appsettings.json -Force
    }
}

Write-Host "===== 全部构建推送完成 =====" -ForegroundColor Green